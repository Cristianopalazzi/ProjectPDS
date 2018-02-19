namespace EasyShare
{
    public static class Constants
    {
        public const int MAX_COUNTER = 1;
        public const string MULTICAST = "239.0.0.222"; // ip multicast
        public const int HELLO_TIME = 2000; 
        public const int CLEAN_TIME = 3 * HELLO_TIME;
        public const int PORT_UDP = 9100; //The port on which to listen for incoming data
        public const int PORT_UDP_IMG = 9050;
        public const int BUFLEN = 300; //Max length of bufferThe port on which to listen for incoming data
        public const string HELL = "HELO";
        public const string QUIT = "QUIT";
        public const int PORT_TCP = 9000;
        public const int PORT_TCP_IMG = 9001;
        public const int FILE_NAME = 256;
        public const string FILE_COMMAND = "FIL";
        public const string ZIP_COMMAND = "ZIP";
        public const string DIR_COMMAND = "DIR";
        public const string ZIP_EXTENSION = ".zip";
        public const string ACCOUNT_IMAGE = @"\Microsoft\Windows\AccountPictures\";
        public const string ACCEPT_FILE = "OK";
        public const string DECLINE_FILE = "NO";
        public const string SETTINGS = "Settings.xml";
        public enum FILE_STATE { PREPARATION = 0 , ACCEPTANCE = 1, PROGRESS = 2, COMPLETED=3, CANCELED=4, ERROR=5, REJECTED=6 };
        public enum NOTIFICATION_STATE { RECEIVED, SENT, CANCELED, REFUSED, NET_ERROR, SEND_ERROR, FILE_ERROR_SEND,FILE_ERROR_REC, REC_ERROR, EXISTS };
        public const string projectName = "EasyShare";
        public const string UTENTE_ANONIMO = "Utente anonimo";
        public const int PACKET_SIZE = 8 * 1024;
    }
}