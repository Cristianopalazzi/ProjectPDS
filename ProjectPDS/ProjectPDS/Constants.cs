namespace ProjectPDS
{
    static class Constants
    {
        public const int MAX_COUNTER = 1;
        public const string MULTICAST = "239.0.0.222"; // ip multicast
        public const int HELLO_TIME = 1000;
        public const int CLEAN_TIME = 6 * HELLO_TIME;
        public const int PORT_UDP = 8888; //The port on which to listen for incoming data
        public const int PORT_UDP_IMG = 8889;
        public const int BUFLEN = 300; //Max length of bufferThe port on which to listen for incoming data
        public const string HELL = "HELL";
        public const string QUIT = "QUIT";
        public const int PORT_TCP = 8080;
        public const int PORT_TCP_IMG = 8081;
        public const int FILE_NAME = 256;
        public const string FILE_COMMAND = "FIL";
        public const string ZIP_COMMAND = "ZIP";
        public const string DEFAULT_DIRECTORY = @"C:\Users\Cristiano\Desktop";
        public const string DIR_COMMAND = "DIR";
        public const string ZIP_EXTENSION = ".zip";
        public const string  NEED_IMG = "YIMG";
        public const string DONT_NEED_IMG = "NIMG";
        //TODO controllare questi path in altre versioni di windows
        public const string ACCOUNT_IMAGE = @"\Microsoft\Windows\AccountPictures\";
        public const string PLACEHOLDER_IMAGE = @"C:\ProgramData\Microsoft\User Account Pictures\guest.png";

    }
}
