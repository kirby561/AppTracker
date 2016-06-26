using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTracker {
    class AtomicFileManager {
        /// <summary>
        /// Writes a file atomically to disk assuming it is read using ReadAtomicFile.
        ///     Note that this isn't truly atomic unless it flushes the OS cache to disk
        ///     inbetween writes.  This will be added later because we'll probably also
        ///     want to do this on another thread so as not to destroy performance.
        /// </summary>
        /// <param name="fileName">The filename to write to.</param>
        /// <param name="fileContents">A string to write into the file.</param>
        /// <returns>Returns true if the write succeeded.</returns>
        public static bool WriteAtomicFile(String fileName, String fileContents) {
            bool result = true;

            try {
                String tempFile = fileName + ".tempfile";
                String newFile = fileName + ".newfile";

                // Delete any temp files that exist
                File.Delete(tempFile);
                File.Delete(newFile);

                // First write all contents to the temp file and flush
                File.WriteAllText(tempFile, fileContents);

                File.Move(tempFile, newFile);
                File.Delete(fileName);
                File.Move(newFile, fileName);
            } catch (IOException exception) {
                Console.WriteLine(exception.Message);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Reads a file that has been written with WriteAtomicFile atomicly.
        /// </summary>
        /// <param name="fileName">The file name to read.</param>
        /// <returns>Returns the read string or null if there was an error</returns>
        public static String ReadAtomicFile(String fileName) {
            String result = null;

            try {
                String tempFile = fileName + ".tempfile";
                String newFile = fileName + ".newfile";

                // Delete the temp file if it exists
                File.Delete(tempFile);

                // If the new file exists, read that
                if (File.Exists(newFile)) {
                    File.Delete(fileName);
                    File.Move(newFile, fileName);
                }

                // Read the file
                result = File.ReadAllText(fileName);
            } catch (IOException exception) {
                Console.WriteLine(exception.Message);
            }

            return result;
        }
    }
}
