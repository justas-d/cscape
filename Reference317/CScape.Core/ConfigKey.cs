namespace CScape.Core
{
    public static class ConfigKey
    {
        public static string Version = "Core_Version";
        public static string Revision = "Core_Revision";

        public static string EntitySystemIdThreshold = "Core_EntitySystem_IdThreshold";
        
        public static string MaxNpcs = "Core_Npcs_Max";
        public static string MaxPlayers = "Core_Players_Max";

        public static string TickRate = "Core_Loop_Tickrate";
        public static string EntityGcInternalMs = "Core_Loop_GcIntervalMs";

        public static string Greeting = "Core_Greeting";

        public static string SocketReceiveTimeout = "Core_Socket_ReceiveTimeout";
        public static string ListenEndPoint = "Core_Socket_ListenEndpoint";
        public static string SocketBacklog = "Core_Socket_Backlog";
        public static string SocketSendTimeout = "Core_Socket_SendTimout";

        public static string PrivateLoginKeyDir = "Core_PrivateKeyDir";
    }
}
