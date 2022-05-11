namespace ECS {
    public delegate void EntityAction<T>(ref T arg);

    public delegate void EntityAction<T0, T1>(ref T0 arg0, ref T1 arg1);

    public delegate void EntityAction<T0, T1, T2>(ref T0 arg0, ref T1 arg1, ref T2 arg2);

    public delegate void EntityAction<T0, T1, T2, T3>(ref T0 arg0, ref T1 arg1, ref T2 arg2, ref T3 arg3);

    public delegate void EntityAction<T0, T1, T2, T3, T4>(ref T0 arg0, ref T1 arg1, ref T2 arg2, ref T3 arg3, ref T4 arg4);

    public delegate void EntityAction<T0, T1, T2, T3, T4, T5>(ref T0 arg0, ref T1 arg1, ref T2 arg2, ref T3 arg3, ref T4 arg4, ref T5 arg5);

    public delegate void EntityAction<T0, T1, T2, T3, T4, T5, T6>(ref T0 arg0, ref T1 arg1, ref T2 arg2, ref T3 arg3, ref T4 arg4, ref T5 arg5,
        ref T6 arg6);

    public delegate void EntityAction_E<T>(in Entity entity, ref T arg);

    public delegate void EntityAction_E<T0, T1>(in Entity entity, ref T0 arg0, ref T1 arg1);

    public delegate void EntityAction_E<T0, T1, T2>(in Entity entity, ref T0 arg0, ref T1 arg1, ref T2 arg2);
    
    public delegate void EntityAction_E<T0, T1, T2, T3>(in Entity entity, ref T0 arg0, ref T1 arg1, ref T2 arg2, ref T3 arg3);

    public delegate void EntityAction_E<T0, T1, T2, T3, T4>(in Entity entity, ref T0 arg0, ref T1 arg1, ref T2 arg2, ref T3 arg3, ref T4 arg4);
    
    public delegate void EntityAction_E<T0, T1, T2, T3, T4, T5>(in Entity entity, ref T0 arg0, ref T1 arg1, ref T2 arg2, ref T3 arg3, ref T4 arg4, ref T5 arg5);
}
