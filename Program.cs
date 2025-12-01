//using System;
//using System.Text;
//using System.Linq;// used for LINQ operations
//using System.Collections.Generic;// collection if i decide to use them
//using System.Diagnostics; // Added for Stopwatch
//using System.Threading.Tasks; // Added for Parallel processing
//using System.Threading;       // Added for synchronization (optional, but good practice)

//public class RC4PinCracker
//{
//    // --- Data Provided by the Challenge ---
//    private static readonly byte[] FixedIV = new byte[]
//     {
//        0xd4, 0x59, 0xca, 0xfa, 0xce, 0x29, 0x8b, 0xbb
//     };

//    // **FIXED CIPHERTEXT:** Using the complete, correct 30-byte ciphertext required to recover the 4-digit PIN.
//    private static readonly string CiphertextHex = "0cfd567edc556b86ec2fe5ed73f302cc71816605e19898eff837172a";
   
//    private static readonly string KnownPlaintext = "Our secret PIN code is: "; // 26 bytes
//    private const int KeyLength = 4; // 4-letter key
//    private const int KeystreamMatchLength = 26; // Match length based on known plaintext
//    private const int TotalKeys = 26 * 26 * 26 * 26; // 456,976

//    // --- RC4 Implementation ---

//    /// <summary>
//    /// RC4 Key Scheduling Algorithm (KSA)
//    /// </summary>
//    private static byte[] KSA(byte[] key)
//    {
//        byte[] S = new byte[256];
//        for (int i = 0; i < 256; i++)
//        {
//            S[i] = (byte)i;
//        }

//        int j = 0;
//        for (int i = 0; i < 256; i++)
//        {
//            j = (j + S[i] + key[i % key.Length]) % 256;
//            // Swap S[i] and S[j]
//            byte temp = S[i];
//            S[i] = S[j];
//            S[j] = temp;
//        }
//        return S;
//    }
//    public static byte[] Rc4Keystream24(string key4)
//    {
//        if (key4 == null || key4.Length != 4)
//            throw new ArgumentException("Key must be exactly 4 characters.");

//        // Convert the 4-letter word to ASCII bytes
//        byte[] keyBytes = Encoding.ASCII.GetBytes(key4);

//        // Build IV || key
//        byte[] fullKey = new byte[FixedIV.Length + keyBytes.Length];
//        Buffer.BlockCopy(FixedIV, 0, fullKey, 0, FixedIV.Length);
//        Buffer.BlockCopy(keyBytes, 0, fullKey, FixedIV.Length, keyBytes.Length);

//        // --- RC4 KSA ---
//        byte[] S = new byte[256];
//        for (int i = 0; i < 256; i++)
//            S[i] = (byte)i;

//        int j = 0;
//        for (int i = 0; i < 256; i++)
//        {
//            j = (j + S[i] + fullKey[i % fullKey.Length]) & 0xFF;
//            (S[i], S[j]) = (S[j], S[i]); // swap
//        }

//        // --- RC4 PRGA ---
//        byte[] keystream = new byte[24];
//        int x = 0, y = 0;

//        for (int n = 0; n < 24; n++)
//        {
//            x = (x + 1) & 0xFF;
//            y = (y + S[x]) & 0xFF;
//            (S[x], S[y]) = (S[y], S[x]); // swap

//            keystream[n] = S[(S[x] + S[y]) & 0xFF];
//        }

//        return keystream;
//    }

//    public static byte[] StringToByteArray(string hex)
//    {
//        return Enumerable.Range(0, hex.Length)
//                         .Where(x => x % 2 == 0)
//                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
//                         .ToArray();
//    }


//    /// <summary>
//    /// RC4 Pseudo-Random Generation Algorithm (PRGA) to generate keystream.
//    /// Note: This is now thread-safe as S is passed by value (copied) from KSA.
//    /// </summary>
//    private static byte[] PRGA(byte[] S, int length)
//    {
//        // Must clone S to ensure thread isolation, as the PRGA modifies S state.
//        byte[] S_clone = (byte[])S.Clone();

//        byte[] keystream = new byte[length];
//        int i = 0;
//        int j = 0;

//        for (int k = 0; k < length; k++)
//        {
//            i = (i + 1) % 256;
//            j = (j + S_clone[i]) % 256;

//            // Swap S[i] and S[j]
//            byte temp = S_clone[i];
//            S_clone[i] = S_clone[j];
//            S_clone[j] = temp;

//            int t = (S_clone[i] + S_clone[j]) % 256;
//            keystream[k] = S_clone[t];
//        }
//        return keystream;
//    }

//    /// <summary>
//    /// Generates the RC4 keystream for a given key and length.
//    /// The RC4 key is IV || sharedSecret.
//    /// </summary>
//    private static byte[] GenerateKeystream(byte[] iv, byte[] sharedSecret, int length)
//    {
//        // Concatenate IV and shared secret to form the RC4 key
//        byte[] rc4Key = iv.Concat(sharedSecret).ToArray();

//        // Perform KSA
//        byte[] S = KSA(rc4Key);

//        // Perform PRGA and return the keystream
//        return PRGA(S, length);
//    }

   

//    /// <summary>
//    /// XORs two byte arrays. They must be the same length.
//    /// </summary>
//    private static byte[] Xor(byte[] IV, byte[] c)
//    {
        

//        byte[] keystream = new byte[IV.Length];
//        for (int i = 0; i < IV.Length; i++)
//        {
//            keystream[i] = (byte)(IV[i] ^ c[i]);
//        }
//        return keystream;
//    }

//    // --- Main Logic ---

//    public static void Main(string[] args)
//    {
//        Console.WriteLine("--- WEP/RC4 Known-Plaintext Attack (Parallel) ---");

//        // 1. Setup Input Data

//        byte[] cBytes = StringToByteArray(CiphertextHex);
//        byte[] mKnownBytes = Encoding.ASCII.GetBytes(KnownPlaintext);

//        // --- DEBUGGING OUTPUT ---
//        Console.WriteLine($"\n[DEBUG] IV byte length: {FixedIV.Length}");
//        Console.WriteLine($"[DEBUG] Ciphertext byte length: {cBytes.Length}");
//        Console.WriteLine($"[DEBUG] Known Plaintext byte length: {mKnownBytes.Length}");
//        // --- END DEBUGGING OUTPUT ---


//        //getting the keystream bytes

//        // The cBytes array has a length of 30. We only need the first 24 bytes to XOR with the mKnownBytes.
//        //byte[] cKnownBytes = cBytes.Take(KeystreamMatchLength).ToArray();

//        // 2. Calculate Target Keystream (This XOR is now guaranteed to work as both arrays are 26 bytes long)
//        // byte[] targetKeystream = Xor(cKnownBytes, mKnownBytes);
//        Console.WriteLine($"\n[INFO] Known Plaintext Length: {KeystreamMatchLength} bytes");
//        Console.WriteLine($"[INFO] Total Keys to Check: {TotalKeys:N0}");
//        // Console.WriteLine($"[INFO] Target Keystream (First {KeystreamMatchLength} bytes): {BytesToHex(targetKeystream)}");

//        // 3. Brute-Force Attack (Parallel)
//        Console.WriteLine("\n[ATTACK] Starting Brute-Force Search...");
//        Stopwatch sw = Stopwatch.StartNew();
//        string[] allwords = WordGenerator.GenerateAll4LetterWords();
//        byte[] foundKeyBytes = null;
//        string foundKeyString = null;
//        // Synchronization object

//        // Use Parallel.For to distribute the workload across multiple cores
//        for (int i = 0; i < allwords.Length; i++)
//        {
//            // Early exit if key is already found
//            if (foundKeyString != null)
//                break;

//            string testKey = allwords[i];
//            byte[] potentialKeyBytes = Encoding.ASCII.GetBytes(testKey);

//            SafeFileWriter.WriteLineSafe(testKey);

//            // Generate the required segment of the keystream
//            byte[] generatedKeystreamSegment = Rc4Keystream24(testKey);

//            byte[] potentialM = Xor(generatedKeystreamSegment, cBytes);
//            string potentialMString = Encoding.ASCII.GetString(potentialM);

//            // Compare with the known plaintext
//            if (potentialMString.Contains(KnownPlaintext))
//            {

//                if (foundKeyString == null) // Double-check
//                {
//                    foundKeyBytes = potentialKeyBytes;
//                    foundKeyString = testKey;
//                }
//            }


//        } 
      

//        sw.Stop();

//        // 4. Report Key Recovery
//        if (foundKeyBytes != null)
//        {
//            Console.WriteLine($"\n[SUCCESS] Key found: {foundKeyString}");
//            Console.WriteLine($"[TIME] Time elapsed for search: {sw.ElapsedMilliseconds:N0} ms");

//            // 5. Decrypt the Full Message
//            byte[] fullKeystream = GenerateKeystream(FixedIV, foundKeyBytes, cBytes.Length);
//            byte[] mFullBytes = Xor(cBytes, fullKeystream);
//            string mFull = Encoding.ASCII.GetString(mFullBytes);

//            // The PIN is the last 4 characters
//            string pin = mFull.Substring(mFull.Length - 4);

//            Console.WriteLine($"\n[DECRYPTION] Full Plaintext Message: {mFull}");
//            Console.WriteLine($"[FINAL PIN] The recovered PIN is: {pin}");
//        }
//        else
//        {
//            Console.WriteLine($"\n[FAILURE] Key not found after checking {TotalKeys:N0} keys.");
//            Console.WriteLine($"[TIME] Total search time: {sw.ElapsedMilliseconds:N0} ms");
//        }
//    }
//}


