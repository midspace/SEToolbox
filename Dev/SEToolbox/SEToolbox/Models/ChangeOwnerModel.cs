namespace SEToolbox.Models
{
    using System.Collections.ObjectModel;
    using System.Linq;

    using SEToolbox.Interop;

    public class ChangeOwnerModel : BaseModel
    {
        #region Fields

        private ObservableCollection<OwnerModel> _playerList;
        private OwnerModel _selectedPlayer;
        private string _title;

        #endregion

        #region ctor

        public ChangeOwnerModel()
        {
            _playerList = new ObservableCollection<OwnerModel>();
        }

        #endregion

        #region Properties

        public ObservableCollection<OwnerModel> PlayerList
        {
            get { return _playerList; }

            set
            {
                if (value != _playerList)
                {
                    _playerList = value;
                    OnPropertyChanged(nameof(PlayerList));
                }
            }
        }

        public OwnerModel SelectedPlayer
        {
            get { return _selectedPlayer; }

            set
            {
                if (value != _selectedPlayer)
                {
                    _selectedPlayer = value;
                    OnPropertyChanged(nameof(SelectedPlayer));
                }
            }
        }

        public string Title
        {
            get { return _title; }

            set
            {
                if (value != _title)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        #endregion

        #region methods

        public void Load(long initalOwner)
        {
            PlayerList.Clear();
            PlayerList.Add(new OwnerModel() { Name = "{None}", PlayerId = 0 });

            foreach (var identity in SpaceEngineersCore.WorldResource.Checkpoint.Identities.OrderBy(p => p.DisplayName))
            {
                if (SpaceEngineersCore.WorldResource.Checkpoint.AllPlayersData != null)
                {
                    var player = SpaceEngineersCore.WorldResource.Checkpoint.AllPlayersData.Dictionary.FirstOrDefault(kvp => kvp.Value.IdentityId == identity.PlayerId);
                    PlayerList.Add(new OwnerModel() { Name = identity.DisplayName, PlayerId = identity.PlayerId, Model = identity.Model, IsPlayer = player.Value != null });
                }
            }

            SelectedPlayer = PlayerList.FirstOrDefault(p => p.PlayerId == initalOwner);
        }

        #endregion
    }
}
