namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.XPath;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Models;
    using SEToolbox.Support;
    using VRage.Game;
    using Res = SEToolbox.Properties.Resources;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;

    public static class SpaceEngineersRepair
    {
        public static string RepairSandBox(WorldResource world)
        {
            var str = new StringBuilder();
            var statusNormal = true;
            var missingFiles = false;
            var saveAfterScan = false;

            // repair will use the WorldResource, thus it won't have access to the wrapper classes.
            // Any repair must be on the raw XML or raw serialized classes.

            var repairWorld = world;
            repairWorld.LoadCheckpoint();

            var xDoc = repairWorld.LoadSectorXml();

            if (xDoc == null)
            {
                str.AppendLine(Res.ClsRepairSectorBroken);
                str.AppendLine(Res.ClsRepairUnableToRepair);
                missingFiles = true;
            }
            else
            {
                var nsManager = xDoc.BuildXmlNamespaceManager();
                var nav = xDoc.CreateNavigator();

                #region Updates the Group Control format.

                var shipNodes = nav.Select("MyObjectBuilder_Sector/SectorObjects/MyObjectBuilder_EntityBase[@xsi:type='MyObjectBuilder_CubeGrid']", nsManager);
                while (shipNodes.MoveNext())
                {
                    var groupBlocksNode = shipNodes.Current.SelectSingleNode("BlockGroups/MyObjectBuilder_BlockGroup/Blocks", nsManager);
                    if (groupBlocksNode != null)
                    {
                        var entityIdNodes = groupBlocksNode.Select("long", nsManager);
                        var removeNodes = new List<XPathNavigator>();
                        while (entityIdNodes.MoveNext())
                        {
                            var entityId = Convert.ToInt64(entityIdNodes.Current.Value);
                            var node = shipNodes.Current.SelectSingleNode(string.Format("CubeBlocks/*[./EntityId='{0}']", entityId), nsManager);
                            if (node != null)
                            {
                                var x = node.ToValue<string>("Min/@x");
                                var y = node.ToValue<string>("Min/@y");
                                var z = node.ToValue<string>("Min/@z");

                                entityIdNodes.Current.InsertBefore(string.Format("<Vector3I><X>{0}</X><Y>{1}</Y><Z>{2}</Z></Vector3I>", x, y, z));
                                removeNodes.Add(entityIdNodes.Current.Clone());
                                str.AppendLine(Res.ClsRepairReplacedBlockGroup);
                                saveAfterScan = true;
                                statusNormal = false;
                            }
                        }

                        foreach (var node in removeNodes)
                        {
                            node.DeleteSelf();
                        }
                    }
                }

                //<BlockGroups>
                //<MyObjectBuilder_BlockGroup>
                //    <Name>Open</Name>
                //    <Blocks>
                //    <long>-2287829012813351669</long>
                //    <long>-1828477283611406765</long>
                //    <long>73405095007807299</long>
                //    <long>-8785290580748247313</long>
                //    </Blocks>
                //</MyObjectBuilder_BlockGroup>
                //</BlockGroups>

                //<BlockGroups>
                //<MyObjectBuilder_BlockGroup>
                //    <Name>Open</Name>
                //    <Blocks>
                //    <Vector3I>
                //        <X>-1</X>
                //        <Y>2</Y>
                //        <Z>-4</Z>
                //    </Vector3I>
                //    <Vector3I>
                //        <X>-1</X>
                //        <Y>7</Y>
                //        <Z>2</Z>
                //    </Vector3I>
                //    <Vector3I>
                //        <X>-1</X>
                //        <Y>8</Y>
                //        <Z>-9</Z>
                //    </Vector3I>
                //    <Vector3I>
                //        <X>-1</X>
                //        <Y>13</Y>
                //        <Z>-3</Z>
                //    </Vector3I>
                //    </Blocks>
                //</MyObjectBuilder_BlockGroup>
                //</BlockGroups>

                if (saveAfterScan)
                {
                    repairWorld.SaveSectorXml(true, xDoc);
                    str.AppendLine(Res.ClsRepairSavedChanges);
                }

                #endregion
            }

            repairWorld.LoadDefinitionsAndMods();
            repairWorld.LoadSector();

            if (repairWorld.Checkpoint == null)
            {
                statusNormal = false;
                str.AppendLine(Res.ClsRepairCheckpointBroken);
                str.AppendLine(Res.ClsRepairUnableToRepair);
                missingFiles = true;
            }

            if (repairWorld.SectorData == null)
            {
                statusNormal = false;
                str.AppendLine(Res.ClsRepairSectorBroken);
                str.AppendLine(Res.ClsRepairUnableToRepair);
                missingFiles = true;
            }

            if (!missingFiles)
            {
                MyObjectBuilder_Character character;

                saveAfterScan = false;

                Dictionary<long, long> idReplacementTable = new Dictionary<long, long>();
                if (repairWorld.Checkpoint.Identities != null)
                {
                    foreach(var identity in repairWorld.Checkpoint.Identities)
                    {
                        if (!SpaceEngineersApi.ValidateEntityType(IDType.IDENTITY, identity.IdentityId))
                        {
                            identity.IdentityId = MergeId(identity.IdentityId, IDType.IDENTITY, ref idReplacementTable);

                            statusNormal = false;
                            str.AppendLine(Res.ClsRepairFixedPlayerIdentity);
                            saveAfterScan = true;
                        }
                    }
                }

                if (repairWorld.Checkpoint.AllPlayersData != null)
                {
                    foreach (var player in repairWorld.Checkpoint.AllPlayersData.Dictionary)
                    {
                        if (!SpaceEngineersApi.ValidateEntityType(IDType.IDENTITY, player.Value.IdentityId))
                        {
                            player.Value.IdentityId = MergeId(player.Value.IdentityId, IDType.IDENTITY, ref idReplacementTable);

                            statusNormal = false;
                            str.AppendLine(Res.ClsRepairFixedPlayerIdentity);
                            saveAfterScan = true;
                        }
                    }
                }

                if (saveAfterScan)
                {
                    repairWorld.SaveCheckPointAndSector(true);
                    str.AppendLine(Res.ClsRepairSavedChanges);
                }

                if (world.SaveType == SaveWorldType.Local)
                {
                    var player = repairWorld.FindPlayerCharacter();

                    if (player == null)
                    {
                        statusNormal = false;
                        str.AppendLine(Res.ClsRepairNoPlayerFound);

                        character = repairWorld.FindAstronautCharacter();
                        if (character != null)
                        {
                            repairWorld.Checkpoint.ControlledObject = character.EntityId;
                            repairWorld.Checkpoint.CameraController = MyCameraControllerEnum.Entity;
                            repairWorld.Checkpoint.CameraEntity = character.EntityId;
                            str.AppendLine(Res.ClsRepairFoundSetPlayer);
                            repairWorld.SaveCheckPointAndSector(true);
                            str.AppendLine(Res.ClsRepairSavedChanges);
                        }
                        else
                        {
                            var cockpit = repairWorld.FindPilotCharacter();
                            if (cockpit != null)
                            {
                                repairWorld.Checkpoint.ControlledObject = cockpit.EntityId;
                                repairWorld.Checkpoint.CameraController = MyCameraControllerEnum.ThirdPersonSpectator;
                                repairWorld.Checkpoint.CameraEntity = 0;
                                str.AppendLine(Res.ClsRepairFoundSetPlayer);
                                repairWorld.SaveCheckPointAndSector(true);
                                str.AppendLine(Res.ClsRepairSavedChanges);
                            }
                            else
                            {
                                // The game is sucessfully generating new characters now, so this code shouldn't be requried any more.
                                /*
                                str.AppendLine("! Could not find any Player Characters.");
                                character = new MyObjectBuilder_Character();
                                character.EntityId = SpaceEngineersApi.GenerateEntityId(IDType.IDENTITY);
                                character.PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene;
                                character.PositionAndOrientation = new MyPositionAndOrientation(VRageMath.Vector3D.Zero, VRageMath.Vector3.Forward, VRageMath.Vector3.Up);
                                character.CharacterModel = Sandbox.Game.Entities.Character.MyCharacter.DefaultModel;
                                character.ColorMaskHSV = new SerializableVector3(0, -1, 1); // White
                                character.Battery = new MyObjectBuilder_Battery { CurrentCapacity = 0.5f };
                                character.LightEnabled = false;
                                character.HeadAngle = new VRageMath.Vector2();
                                character.LinearVelocity = new VRageMath.Vector3();
                                character.AutoenableJetpackDelay = -1;
                                character.JetpackEnabled = true;
                                character.Inventory = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_Inventory>();

                                // Add default items to Inventory.
                                MyObjectBuilder_InventoryItem item;
                                MyObjectBuilder_EntityBase gunEntity;

                                character.Inventory.Items.Add(item = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_InventoryItem>());
                                item.Amount = 1;
                                item.ItemId = 0;
                                gunEntity = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_Welder>();
                                gunEntity.EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ENTITY);
                                gunEntity.PersistentFlags = MyPersistentEntityFlags2.None;
                                item.PhysicalContent = new MyObjectBuilder_PhysicalGunObject {GunEntity = gunEntity};

                                character.Inventory.Items.Add(item = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_InventoryItem>());
                                item.Amount = 1;
                                item.ItemId = 1;
                                gunEntity = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_AngleGrinder>();
                                gunEntity.EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ENTITY);
                                gunEntity.PersistentFlags = MyPersistentEntityFlags2.None;
                                item.PhysicalContent = new MyObjectBuilder_PhysicalGunObject { GunEntity = gunEntity };

                                character.Inventory.Items.Add(item = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_InventoryItem>());
                                item.Amount = 1;
                                item.ItemId = 2;
                                gunEntity = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_HandDrill>();
                                gunEntity.EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ENTITY);
                                gunEntity.PersistentFlags = MyPersistentEntityFlags2.None;
                                item.PhysicalContent = new MyObjectBuilder_PhysicalGunObject { GunEntity = gunEntity };

                                character.Inventory.Items.Add(item = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_InventoryItem>());
                                item.Amount = 1;
                                item.ItemId = 3;
                                gunEntity = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_AutomaticRifle>();
                                gunEntity.EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ENTITY);
                                gunEntity.PersistentFlags = MyPersistentEntityFlags2.None;
                                item.PhysicalContent = new MyObjectBuilder_PhysicalGunObject { GunEntity = gunEntity };

                                repairWorld.Checkpoint.ControlledObject = character.EntityId;
                                repairWorld.Checkpoint.CameraController = MyCameraControllerEnum.Entity;
                                repairWorld.Checkpoint.CameraEntity = character.EntityId;

                                repairWorld.SectorData.SectorObjects.Add(character);

                                str.AppendLine("* Created new active Player.");
                                repairWorld.SaveCheckPointAndSector(true);
                                str.AppendLine(Res.ClsRepairSavedChanges);
                                */
                            }
                        }
                    }

                    saveAfterScan = false;

                    // Make sure the character in a locally saved world has all tools.
                    // SubtypeNames for required tools.
                    var requiredItems = new[] { 
                        "WelderItem", "AngleGrinderItem", "HandDrillItem" };

                    character = repairWorld.FindAstronautCharacter()
                        ?? repairWorld.FindPilotCharacter().Pilot;

                    var inventoryBase = character.ComponentContainer.Components.FirstOrDefault(e => e.TypeId == "MyInventoryBase");
                    MyObjectBuilder_Inventory inventory = inventoryBase == null ? null : inventoryBase.Component as MyObjectBuilder_Inventory;

                    if (inventory != null)
                    {
                        requiredItems.ForEach(
                            delegate(string subtypeName)
                            {
                                if (!inventory.Items.Any(i =>
                                    i.PhysicalContent != null &&
                                    i.PhysicalContent.SubtypeName == subtypeName))
                                {
                                    statusNormal = false;
                                    str.AppendLine(string.Format(Res.ClsRepairReplacedCharacterSubType, subtypeName));
                                    saveAfterScan = true;
                                    inventory.Items.Add(new MyObjectBuilder_InventoryItem
                                    {
                                        Amount = 1,
                                        PhysicalContent = new MyObjectBuilder_PhysicalGunObject {SubtypeName = subtypeName},
                                        ItemId = inventory.nextItemId,
                                    });
                                    inventory.nextItemId++;
                                }
                            });
                    }
                }

                // Scan through all items.
                foreach (var entity in repairWorld.SectorData.SectorObjects)
                {
                    if (entity is MyObjectBuilder_CubeGrid)
                    {
                        var cubeGrid = (MyObjectBuilder_CubeGrid)entity;

                        var list = cubeGrid.CubeBlocks.OfType<MyObjectBuilder_Cockpit>().ToArray();
                        for (var i = 0; i < list.Length; i++)
                        {
                            character = (list[i]).GetHierarchyCharacters().FirstOrDefault();
                            if (character != null)
                            {
                                if (!SpaceEngineersCore.Resources.CharacterDefinitions.Any(c => c.Model == character.CharacterModel || c.Name == character.CharacterModel))
                                {
                                    character.CharacterModel = Sandbox.Game.Entities.Character.MyCharacter.DefaultModel;
                                    statusNormal = false;
                                    str.AppendLine(Res.ClsRepairFixedCharacterModel);
                                    saveAfterScan = true;
                                }
                            }
                        }

                        // TODO: search for cubeblocks that don't exist in the definitions.
                        //var definition = SpaceEngineersAPI.GetCubeDefinition(block.GetType(), CubeGrid.GridSizeEnum, block.SubtypeName);
                    }

                    character = entity as MyObjectBuilder_Character;
                    if (character != null)
                    {
                        if (!SpaceEngineersCore.Resources.CharacterDefinitions.Any(c => c.Model == character.CharacterModel || c.Name == character.CharacterModel))
                        {
                            character.CharacterModel = Sandbox.Game.Entities.Character.MyCharacter.DefaultModel;
                            statusNormal = false;
                            str.AppendLine(Res.ClsRepairFixedCharacterModel);
                            saveAfterScan = true;
                        }
                    }
                }

                //if (world.Checkpoint.Players != null)
                //{
                //    foreach (var item in world.Checkpoint.Players.Dictionary)
                //    {
                //        if (!SpaceEngineersCore.Resources.CharacterDefinitions.Any(c => c.Name == item.Value.PlayerModel))
                //        {
                //            item.Value.PlayerModel = SpaceEngineersCore.Resources.CharacterDefinitions[0].Name;
                //            statusNormal = false;
                //            str.AppendLine(Res.ClsRepairFixedCharacterModel);
                //            saveAfterScan = true;
                //        }

                //        // AllPlayers is obsolete.
                //        //if (item.Value.PlayerId == 0)
                //        //{
                //        //    item.Value.PlayerId = SpaceEngineersApi.GenerateEntityId();
                //        //    world.Checkpoint.AllPlayers.Add(new MyObjectBuilder_Checkpoint.PlayerItem(item.Value.PlayerId, "Repair", false, item.Value.SteamID, null));
                //        //    statusNormal = false;
                //        //    str.AppendLine("! Fixed corrupt or missing Player defitinion.");
                //        //    saveAfterScan = true;
                //        //}
                //    }
                //}

                if (saveAfterScan)
                {
                    repairWorld.SaveCheckPointAndSector(true);
                    str.AppendLine(Res.ClsRepairSavedChanges);
                }
            }

            if (statusNormal)
            {
                str.AppendLine(Res.ClsRepairNoIssues);
            }

            return str.ToString();
        }

        private static Int64 MergeId(long currentId, IDType type, ref Dictionary<Int64, Int64> idReplacementTable)
        {
            if (currentId == 0)
                return 0;

            if (idReplacementTable.ContainsKey(currentId))
                return idReplacementTable[currentId];

            idReplacementTable[currentId] = SpaceEngineersApi.GenerateEntityId(type);
            return idReplacementTable[currentId];
        }
    }
}
