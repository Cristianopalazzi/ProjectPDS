namespace ProjectPDS
{
    static class Constants
    {
        public const int MAX_COUNTER = 1;
        public const string MULTICAST = "224.0.0.1"; // ip multicast
        public const int HELLO_TIME = 1000;
        public const int CLEAN_TIME = 6 * HELLO_TIME;
        public const int PORT_UDP = 8888; //The port on which to listen for incoming data
        public const int BUFLEN = 512; //Max length of bufferThe port on which to listen for incoming data
        public const string HELL = "HELL";
        public const string QUIT = "QUIT";
        public const int PORT_TCP = 8080;
        public const int FILE_NAME = 256;
        public const string FILE_COMMAND = "FIL";
        public const string ZIP_COMMAND = "ZIP";
        public const string DEFAULT_DIRECTORY = "C:\\Users\\Cristiano\\Desktop";
        public const string DIR_COMMAND = "DIR";
    }
}
