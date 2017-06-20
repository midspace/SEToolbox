namespace SEToolbox.Support
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    public static class ReflectionUtil
    {
        /// <summary>
        /// Replaces the method.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        public static void ReplaceMethod(MethodBase source, MethodBase dest)
        {
            if (!MethodSignaturesEqual(source, dest))
            {
                throw new ArgumentException("The method signatures are not the same.", nameof(source));
            }
            ReplaceMethod(GetMethodAddress(source), dest);
        }

        /// <summary>
        /// Replaces the method.
        /// </summary>
        /// <param name="srcAdr">The SRC adr.</param>
        /// <param name="dest">The dest.</param>
        public static void ReplaceMethod(IntPtr srcAdr, MethodBase dest)
        {
            IntPtr destAdr = GetMethodAddress(dest);
            unsafe
            {
                if (IntPtr.Size == 8)
                {
                    ulong* d = (ulong*)destAdr.ToPointer();
                    *d = *((ulong*)srcAdr.ToPointer());
                }
                else
                {
                    uint* d = (uint*)destAdr.ToPointer();
                    *d = *((uint*)srcAdr.ToPointer());
                }
            }
        }

        /// <summary>
        /// Gets the address of the method stub
        /// </summary>
        /// <param name="method">The method handle.</param>
        /// <returns></returns>
        public static IntPtr GetMethodAddress(MethodBase method)
        {
            if ((method is DynamicMethod))
            {
                return GetDynamicMethodAddress(method);
            }

            // Prepare the method so it gets jited
            RuntimeHelpers.PrepareMethod(method.MethodHandle);
            unsafe
            {
                return new IntPtr((int*)method.MethodHandle.Value.ToPointer() + 2);
            }
        }

        private static IntPtr GetDynamicMethodAddress(MethodBase method)
        {
            unsafe
            {
                RuntimeMethodHandle handle = GetDynamicMethodRuntimeHandle(method);
                byte* ptr = (byte*)handle.Value.ToPointer();
                if (IntPtr.Size == 8)
                {
                    ulong* address = (ulong*)ptr;
                    address += 6;
                    return new IntPtr(address);
                }
                else
                {
                    uint* address = (uint*)ptr;
                    address += 6;
                    return new IntPtr(address);
                }


            }
        }

        private static RuntimeMethodHandle GetDynamicMethodRuntimeHandle(MethodBase method)
        {
            if (method is DynamicMethod)
            {
                FieldInfo fieldInfo = typeof(DynamicMethod).GetField("m_method", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    RuntimeMethodHandle handle = ((RuntimeMethodHandle)fieldInfo.GetValue(method));

                    return handle;
                }
            }
            return method.MethodHandle;
        }

        private static bool MethodSignaturesEqual(MethodBase x, MethodBase y)
        {
            if (x.CallingConvention != y.CallingConvention)
            {
                return false;
            }
            Type returnX = GetMethodReturnType(x), returnY = GetMethodReturnType(y);
            if (returnX != returnY)
            {
                return false;
            }
            ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
            if (xParams.Length != yParams.Length)
            {
                return false;
            }
            for (int i = 0; i < xParams.Length; i++)
            {
                if (xParams[i].ParameterType != yParams[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }

        private static Type GetMethodReturnType(MethodBase method)
        {
            MethodInfo methodInfo = method as MethodInfo;
            if (methodInfo != null)
                return methodInfo.ReturnType;

            ConstructorInfo constructorInfo = method as ConstructorInfo;
            if (constructorInfo != null)
                return typeof(void);

            throw new ArgumentException("Unsupported MethodBase : " + method.GetType().Name, nameof(method));
        }

        public static T GetStaticField<T>(this Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return (T)field.GetValue(null);
        }
    }
}