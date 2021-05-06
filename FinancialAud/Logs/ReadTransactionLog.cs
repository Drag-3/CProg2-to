using System;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Runtime.CompilerServices;

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

    public class TransactionLog
    {
        //public static string WinDir= System.Environment.GetEnvironmentVariable("windir");
        public static string WinDir = Environment.CurrentDirectory; // Would be better if I knew how to make a relative path

        private ulong _transactionsCount;
        private FileStream _inputFile;
        private FileStream _outputFile;
        private StreamReader _inputFileStream;
        private StreamWriter _outputFileStream;
        private string _inputFileName;
        private string _outputFileName;
        private bool _filesOpen;


        public bool HasTransactions => HasMoreTransactions();

        public string InputFileName
        {
            get => _inputFileName;
            set
            {
                if (_filesOpen) throw new Exception("Files must be closed first");
                _inputFileName = value;
            }
        }

        public string OutputFileName
        {
            get => _outputFileName;
            set
            {
                if (_filesOpen) throw new Exception("Files must be closed first");
                _outputFileName = value;
            }
        }

        public TransactionLog()
        {
            _filesOpen = false;
            _transactionsCount = 0;
        }

        public TransactionLog(string inputFileName)
        {
            _filesOpen = false;
            _inputFileName = inputFileName;
            _transactionsCount = 0;
            _inputFile = null; // Do I even need to do this ? 
            _outputFile = null; // Not created Yet
            _outputFileName = string.Empty;
        }

        public TransactionLog(string inputFileName, string outputFileName)
        {
            _filesOpen = false;
            _inputFileName = inputFileName;
            _outputFileName = outputFileName;
            _inputFile = null;
            _outputFile = null;
            _transactionsCount = 0;
        }
        
        ~TransactionLog() 
        {
            _inputFile?.Dispose(); // I learn that this checks if null and then calls the method. Neat
            _outputFile?.Dispose();
            _inputFileStream?.Dispose();
            _outputFileStream?.Dispose();
        }

        public bool OpenFiles()
        {
            if (!_filesOpen)
            {
                try
                {
                    _inputFile =
                        File.Open(WinDir + "//" + _inputFileName, FileMode.Open,
                            FileAccess.Read); // Maybe I should add something like.txt
                    _inputFileStream = new StreamReader(_inputFile);
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

                if (_outputFileName == string.Empty)
                {
                    _filesOpen = true;
                    return true;
                }
                else
                {
                    try
                    {
                        _outputFile = File.Open(WinDir + "//" + _outputFileName, FileMode.Create, FileAccess.Write);
                        _outputFileStream = new StreamWriter(_outputFile);
                        _filesOpen = true;
                        return true;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _inputFile.Dispose(); // will have been made by this point
                        _inputFileStream.Dispose();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Input file does not exist");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Gray;
                        throw;
                    }
                    catch (IOException)
                    {
                        _inputFile.Dispose(); // will have been made by this point
                        _inputFileStream.Dispose();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Input file does not exist");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Gray;
                        throw;
                    }
                    catch (ArgumentException)
                    {
                        _inputFile.Dispose();
                        _inputFileStream.Dispose();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid File name");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Gray;
                        throw;
                    }
                    catch (Exception)
                    {
                        _inputFile.Dispose();
                        _inputFileStream.Dispose();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Something Unexpected Happened");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Gray;
                        throw;
                    }
                }
            }

            return false;
        }

        public void WriteLogEntry(string entryToEnter)
        {
            if (_outputFileName == string.Empty) return;
            if (_filesOpen)
            {
                //using var output = new StreamWriter(_outputFile);
                _outputFileStream.Write(entryToEnter);
                _outputFileStream.Flush();

            }
            else
            {
                throw new Exception("File must be opened First");
            }
        }

        public bool HasMoreTransactions()
        {
            if (!_filesOpen)
            {
                throw new Exception("File must be opened first");
            }

            if (_inputFile == null) return false;
            //if (_inputFileStream.Peek() < 0) return false;
            //Console.WriteLine(_inputFileStream.Peek());
            //Console.WriteLine(_inputFileStream.Peek());
            //Console.WriteLine(_inputFileStream.Peek());
            //var input = new StreamReader(_inputFile);
            while (_inputFile != null && !(_inputFileStream.Peek() < 0) &&
                   (!(char.IsLetter(Convert.ToChar(_inputFileStream.Peek())) ||
                      char.IsDigit(Convert.ToChar(_inputFileStream.Peek()))) ||
                    Convert.ToChar(_inputFileStream.Peek()) == '#'))
            {
                if (Convert.ToChar(_inputFileStream.Peek()) == '#') // Check for comment
                {
                    _inputFileStream.ReadLine(); // Ignore line
                }
                else
                {
                    _inputFileStream.ReadLine();
                }
            }

            //Console.WriteLine(Convert.ToChar(_inputFileStream.Peek()));
            return _inputFile != null && !(_inputFileStream.Peek() < 0) &&
                   (char.IsLetter(Convert.ToChar((_inputFileStream.Peek()))) ||
                    char.IsDigit(Convert.ToChar((_inputFileStream.Peek()))));
        }

        public Transaction GetNextTransaction()
        {
            if (!_filesOpen) throw new Exception("File must be opened first!");
            if (_inputFile != null)
            {
                var item = new Transaction();
                while (_inputFile != null && !(_inputFileStream.Peek() < 0) &&
                       !(char.IsLetter(Convert.ToChar(_inputFileStream.Peek())) ||
                         char.IsDigit(Convert.ToChar(_inputFileStream.Peek()))))
                {
                    _inputFileStream.Read(); // Ignore Non - alphanumeric characters (eg whitespace) at start of line
                }



                if (_inputFile != null && !(_inputFileStream.Peek() < 0) &&
                    (char.IsLetter(Convert.ToChar(_inputFileStream.Peek())) ||
                     char.IsDigit(Convert.ToChar(_inputFileStream.Peek()))))
                {
                    item.transactionID = ++_transactionsCount; // ID is the transaction number

                    string line = _inputFileStream.ReadLine(); // Gets line and stores in a string
                    var fields = line?.Split(" "); //Get elements of the log
                    if (fields != null)
                        for (var i = 0; i < fields.Length; ++i)
                        {
                            fields[i] = fields[i].Replace('_', ' '); // Fix spaces
                        }

                    item.action =
                        Convert.ToChar(
                            fields[0]); //Complains possible nullPointerExp but didn't I check up there^ why would it change in between?
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
                        case 'Y': item.primaryAccount = Convert.ToBoolean(Convert.ToInt32(fields[2]));
                            break;
                        case 'E': goto case 'M'; // Swap the primary and secondary accounts of a user
                        case 'M': break; // Post interest on all accounts
                        default:
                            throw new Exception("Input File Corruption Detected");

                    }
                }

                return item;
            }

            throw new Exception("Transaction Log Issue");
        }

        public bool CloseFiles()
        {
            if (_filesOpen)
            {
                _inputFileStream?.Dispose();
                _outputFileStream?.Dispose();
                _inputFile?.Dispose();
                _outputFile?.Dispose();
                _filesOpen = false;
                return true;
            }

            return false;
        }
    };
}