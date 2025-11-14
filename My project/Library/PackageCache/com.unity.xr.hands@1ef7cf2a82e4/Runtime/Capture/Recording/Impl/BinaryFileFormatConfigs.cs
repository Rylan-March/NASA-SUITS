namespace UnityEngine.XR.Hands.Capture.Recording
{
    static class XRHandRecordingBinaryFileFormatConfigs
    {
        /***
         * Recording binary file format:
         *
         * - int: version number of the file format             [4 bytes]
         *
         * - string: recording name string                  [variable length]
         *
         * - float: total time of the recording in seconds      [4 bytes]
         *
         * - int: frame count                                   [4 bytes]
         *
         * - For each frame:
         *   - float: timestamp in seconds                      [4 bytes]
         *
         *   - bool: whether the left hand is tracked           [1 byte]
         *   - bool: whether the right hand is tracked          [1 byte]
         *   - bool: whether all the left hand joint are valid  [1 byte]
         *   - bool: whether all the right hand joint are valid [1 byte]
         *
         *   - Left Hand
         *     - If left hand is tracked, for each joint:
         *       - float x, y, z     (position)                 [12 bytes]
         *       - float x, y, z, w  (rotation)                 [16 bytes]
         *
         *  - Right Hand
         *    - If right hand is tracked , For each joint:
         *      - float x, y, z     (position)                  [12 bytes]
         *      - float x, y, z, w  (rotation)                  [16 bytes]
         */

        // Current version of the binary file format, update this when the format changes
        internal const int k_Version = 1;

        // File extension used for recording binary files stored in the device's persistent data directory.
        internal const string k_FileExtension = ".handsbin";
    }
}
