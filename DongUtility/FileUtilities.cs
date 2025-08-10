namespace DongUtility
{
    /// <summary>
    /// Generic file utilities
    /// </summary>
    public static class FileUtilities
    {
        static public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Gets the root directory of the project
        /// </summary>
        static public string GetMainProjectDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory + @"../../../";
        }

        /// <summary>
        /// Checks whether a BinaryReader is currently at end of file
        /// </summary>
        static public bool IsEndOfFile(BinaryReader br)
        {
            // From https://stackoverflow.com/questions/10942848/c-sharp-checking-for-binary-reader-end-of-file
            return br.BaseStream.Position == br.BaseStream.Length;
        }
    }
}
