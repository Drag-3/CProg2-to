using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using Bank.Extentions;


namespace Bank.Logs
{

    using System.IO;

    public struct Transaction
    {
        public ulong transactionID;
        public char action;
        public int customerNumber;
        public bool primaryAccount;
        public int checkNumber;
        public double amountOrRate;
        public int recipientCustomerNumber;
        public bool recipientPrimary;
        public string customerName;
    }

    

    public enum MyFileType
    {
        InputFile,
        DetailFile,
        ErrorFile
    }

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

        public TransactionLog(string inputFileName)
        {
            _inputFileName = inputFileName;
            _transactionsCount = 0;
        }

        public TransactionLog(string inputFileName, string outputFileName)
        {
            _inputFileName = inputFileName;
            _transactionsCount = 0;
        }

        ~TransactionLog()
        {
            if (_inputFile != null && _inputFile.FileOpen) _inputFile?.Dispose();
            if (_detailFile != null && _detailFile.FileOpen) _detailFile?.Dispose();
            if (_errorFile != null && _errorFile.FileOpen) _errorFile?.Dispose();
        }

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
                
                item.transactionID = ++_transactionsCount; // ID is the transaction number

                var line = _inputFile.ReadLine(); // Gets line and stores in a string
                var fields = line?.Split(" "); //Get elements of the log
                if (fields != null)
                    for (var i = 0; i < fields.Length; ++i)
                    {
                        fields[i] = fields[i].Replace('_', ' '); // Fix spaces
                    }
                else return item;
                
                item.action =
                    Convert.ToChar(fields[0]); 
                item.customerNumber =
                    Convert.ToInt32(fields[1]); // Anyway, Action and customerNumber are in all entries

                switch (item.action)
                {
                    case 'A': goto case 'N'; // Add new Customer
                    case 'N': // Change Customer Name
                        item.customerName = fields[2];
                        break;
                    case 'K': // check
                        item.primaryAccount = Convert.ToBoolean(Convert.ToInt32(fields[2]));
                        item.checkNumber = Convert.ToInt32(fields[3]);
                        item.amountOrRate = Convert.ToDouble(fields[4]);
                        item.recipientCustomerNumber = Convert.ToInt32(fields[5]);
                        item.recipientPrimary = Convert.ToBoolean(Convert.ToInt32(fields[6]));
                        item.customerName = fields[7];
                        break;
                    case 'X': //transfer
                        item.primaryAccount = Convert.ToBoolean(Convert.ToInt32(fields[2]));
                        item.amountOrRate = Convert.ToDouble(fields[3]);
                        item.recipientCustomerNumber = Convert.ToInt32(fields[4]);
                        item.recipientPrimary = Convert.ToBoolean(Convert.ToInt32(fields[5]));
                        break;
                    case 'L': // Link Customer Accounts
                        item.recipientCustomerNumber = Convert.ToInt32(fields[2]);
                        item.recipientPrimary = Convert.ToBoolean(Convert.ToInt32(fields[3]));
                        break;
                    case 'D': goto case 'R'; // Deposit to an Account
                    case 'W': goto case 'R'; // Change the apr of an account
                    case 'R':
                        item.primaryAccount = Convert.ToBoolean(Convert.ToInt32(fields[2]));
                        item.amountOrRate = Convert.ToDouble(fields[3]);
                        break;
                    case 'S': goto case 'P'; // Create a new account of type savings
                    case 'C': goto case 'P'; // Create a new account of type checking
                    case 'P': // Change the interest rate of an Created Account
                        //Console.WriteLine(fields[2]);
                        item.amountOrRate = Convert.ToDouble(fields[2]);
                        break;
                    case 'Y':
                        item.primaryAccount = Convert.ToBoolean(Convert.ToInt32(fields[2]));
                        break;
                    case 'E': goto case 'M'; // Swap the primary and secondary accounts of a user
                    case 'M': break; // Post interest on all accounts
                    default:
                        throw new Exception("Input File Corruption Detected");
                }
                return item;
            }
        }

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