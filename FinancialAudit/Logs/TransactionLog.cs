using System;
using System.IO;
using FinancialAudit.Extentions;
using FinancialAudit.IO;

namespace FinancialAudit.Logs
{
    /// <summary>
    /// Struct containing details of a Transaction
    /// </summary>
    public struct Transaction
    {
        public ulong TransactionId { get; set; }
        public char Action { get; set; }
        public int CustomerNumber { get; set; }
        public bool PrimaryAccount { get; set; }
        public int CheckNumber { get; set; }
        public double AmountOrRate{ get; set; }
        public int RecipientCustomerNumber { get; set; }
        public bool RecipientPrimary { get; set; }
        public string CustomerName { get; set; }
    }

    

    
    public enum MyFileType
    {
        InputFile,
        DetailFile,
        ErrorFile
    }

    /// <summary>
    /// Read and parse Transaction Log files
    /// </summary>
    public class TransactionLog
    {
        //public static string WinDir= System.Environment.GetEnvironmentVariable("windir");
        public static string
            WinDir = Environment.CurrentDirectory; // Would be better if I knew how to make a relative path

        private ulong _transactionsCount;
        private TextStream _inputFile;
        private TextStream _detailFile;
        private TextStream _errorFile;
        private string _inputFileName;


        public bool HasTransactions => HasMoreTransactions();

        public string InputFileName
        {
            get => _inputFileName;
            set
            {
                if (_inputFile.FileOpen || _detailFile.FileOpen || _errorFile.FileOpen)
                    throw new Exception("Files must be closed first");
                _inputFileName = value;
            }
        }
        
        public bool InputFileOpen => _inputFile!= null  && _inputFile.FileOpen;
        public bool DetailsFileOpen => _detailFile !=  null && _detailFile.FileOpen;
        public bool ErrorsFileOpen => _errorFile != null && _errorFile.FileOpen;

        public TransactionLog()
        {
            _transactionsCount = 0;
        }

        public TransactionLog(string inputFileName) :this()
        {
            _inputFileName = inputFileName;
        }
        

        ~TransactionLog()
        {
            if (_inputFile != null && _inputFile.FileOpen) _inputFile?.Dispose();
            if (_detailFile != null && _detailFile.FileOpen) _detailFile?.Dispose();
            if (_errorFile != null && _errorFile.FileOpen) _errorFile?.Dispose();
        }

        /// <summary>
        /// If file is not open opens log file with stored filename.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>Returns true if the file is opened by this function</returns>
        public bool OpenInputFile()
        {
            if (_inputFile != null && _inputFile.FileOpen) return false;
            try
            {
                _inputFile =
                    new TextStream(File.Open(WinDir + "//" + _inputFileName, FileMode.Open,
                        FileAccess.Read)); // Maybe I should add something like.txt
                
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Input file does not exist");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                throw;
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Input file does not exist");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                throw;
            }
            catch (ArgumentException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid File name");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                throw;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Something Unexpected Happened");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                throw;
            }


            return _inputFile.FileOpen;

        }

        /// <summary>
        /// Writes a text file with the given file name and message to the working directory
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lineToWrite"></param>
        /// <returns></returns>
        public static bool WriteToOutputFile(string fileName, string lineToWrite)
        {
            using var outFile = new TextStream(File.Open(Environment.CurrentDirectory + $"\\{fileName}", FileMode.Append));
            try
            {
                   outFile.WriteLine(lineToWrite); 
            }
            catch (Exception)

            {
                return false;
            }

            return true;
        }
        
        
        public bool HasMoreTransactions()
        {
            if (!_inputFile.FileOpen)
            {
                throw new Exception("File must be opened first");
            }

            if (_inputFile == null) return false;
            //if (_inputFileStream.Peek() < 0) return false;
            //Console.WriteLine(_inputFileStream.Peek());
            //Console.WriteLine(_inputFileStream.Peek());
            //Console.WriteLine(_inputFileStream.Peek());
            //var input = new StreamReader(_inputFile);
            while (_inputFile != null && _inputFile.Peek() != char.MinValue &&
                   (!_inputFile.Peek().IsAlphaNumeric() || _inputFile.Peek() == '#'))
            {
                if (_inputFile.Peek() == '#') // Check for comment
                {
                    _inputFile.IgnoreLine(); // Ignore line
                }
                else
                {
                    _inputFile.IgnoreLine();
                }
            }

            //Console.WriteLine(Convert.ToChar(_inputFileStream.Peek()));
            return _inputFile != null && _inputFile.Peek() != char.MinValue &&
                   _inputFile.Peek().IsAlphaNumeric();
        }

        /// <summary>
        /// Returns a Transaction Struct containing the details of the next Transaction
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The Transaction stored in the log does not exist</exception>
        public Transaction GetNextTransaction()
        {
            if (_inputFile != null && !_inputFile.FileOpen) throw new Exception("File must be opened first!");
            {
                var item = new Transaction();
                while (_inputFile != null && _inputFile.Peek() != char.MinValue &&
                       !_inputFile.Peek().IsAlphaNumeric())
                {
                    _inputFile.Ignore(); // Ignore Non - alphanumeric characters (eg whitespace) at start of line
                }


                if (_inputFile == null || _inputFile.Peek() == char.MinValue ||
                    !_inputFile.Peek().IsAlphaNumeric()) return item;
                
                item.TransactionId = ++_transactionsCount; // ID is the transaction number

                var line = _inputFile.ReadLine(); // Gets line and stores in a string
                var fields = line?.Split(" "); //Get elements of the log
                if (fields != null)
                    for (var i = 0; i < fields.Length; ++i)
                    {
                        fields[i] = fields[i].Replace('_', ' '); // Fix spaces
                    }
                else return item;
                
                item.Action =
                    Convert.ToChar(fields[0]); 
                item.CustomerNumber =
                    Convert.ToInt32(fields[1]); // Anyway, Action and customerNumber are in all entries

                switch (item.Action)
                {
                    case 'A': goto case 'N'; // Add new Customer
                    case 'N': // Change Customer Name
                        item.CustomerName = fields[2];
                        break;
                    case 'K': // check
                        item.PrimaryAccount = Convert.ToBoolean(Convert.ToInt32(fields[2]));
                        item.CheckNumber = Convert.ToInt32(fields[3]);
                        item.AmountOrRate = Convert.ToDouble(fields[4]);
                        item.RecipientCustomerNumber = Convert.ToInt32(fields[5]);
                        item.RecipientPrimary = Convert.ToBoolean(Convert.ToInt32(fields[6]));
                        item.CustomerName = fields[7];
                        break;
                    case 'X': //transfer
                        item.PrimaryAccount = Convert.ToBoolean(Convert.ToInt32(fields[2]));
                        item.AmountOrRate = Convert.ToDouble(fields[3]);
                        item.RecipientCustomerNumber = Convert.ToInt32(fields[4]);
                        item.RecipientPrimary = Convert.ToBoolean(Convert.ToInt32(fields[5]));
                        break;
                    case 'L': // Link Customer Accounts
                        item.RecipientCustomerNumber = Convert.ToInt32(fields[2]);
                        item.RecipientPrimary = Convert.ToBoolean(Convert.ToInt32(fields[3]));
                        break;
                    case 'D': goto case 'R'; // Deposit to an Account
                    case 'W': goto case 'R'; // Change the apr of an account
                    case 'R':
                        item.PrimaryAccount = Convert.ToBoolean(Convert.ToInt32(fields[2]));
                        item.AmountOrRate = Convert.ToDouble(fields[3]);
                        break;
                    case 'S': goto case 'P'; // Create a new account of type savings
                    case 'C': goto case 'P'; // Create a new account of type checking
                    case 'P': // Change the interest rate of an Created Account
                        //Console.WriteLine(fields[2]);
                        item.AmountOrRate = Convert.ToDouble(fields[2]);
                        break;
                    case 'Y':
                        item.PrimaryAccount = Convert.ToBoolean(Convert.ToInt32(fields[2]));
                        break;
                    case 'E': goto case 'M'; // Swap the primary and secondary accounts of a user
                    case 'M': break; // Post interest on all accounts
                    default:
                        throw new ArgumentException("Input File Corruption Detected");
                }
                return item;
            }
        }

        /// <summary>
        /// Closes a specified file if it is open
        /// </summary>
        /// <param name="file"></param>
        /// <returns>bool representing success of closure </returns>
        /// <exception cref="ArgumentException">file type does not exist</exception>
        public bool CloseFile(MyFileType file)
        {
            switch (file)
            {
                case MyFileType.InputFile:
                    if (_inputFile == null || !_inputFile.FileOpen) return false;
                    _inputFile?.Dispose();
                    return true;
                case MyFileType.DetailFile:
                    if (_detailFile == null || !_detailFile.FileOpen) return false;
                    _detailFile?.Dispose();
                    return true;
                case MyFileType.ErrorFile:
                    if ( _errorFile == null || !_errorFile.FileOpen) return false;
                    _errorFile?.Dispose();
                    return true;
                default:
                    throw new ArgumentException("Invalid File");
            }
            
        }

        public bool OpenDetailsLog()
        {
            if (string.IsNullOrEmpty(_inputFileName)) return false;
            var detailsFileName = _inputFileName + "-detail.log";
            try
            {
                _detailFile = new TextStream(File.Open(WinDir + detailsFileName, FileMode.CreateNew));
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The file could not be created");
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (ArgumentException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Invalid characters in path ({detailsFileName})");
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (UnauthorizedAccessException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File Exists and cannot be opened to format (Or is a directory) {detailsFileName}");
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
           
            return _detailFile.FileOpen;
        }
        
        public bool OpenErrorLog()
        {
            if (string.IsNullOrEmpty(_inputFileName)) return false;
            var errorFileName = _inputFileName + "-error.log";
            try
            {
                _errorFile = new TextStream(File.Open(WinDir + errorFileName, FileMode.CreateNew));
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The file could not be created");
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (ArgumentException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Invalid characters in path ({errorFileName})");
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (UnauthorizedAccessException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File Exists and cannot be opened to format (Or is a directory) {errorFileName}");
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
           
            return _errorFile.FileOpen;
        }
    };
}