using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TextStreamReader
{
    public enum NewLine
    {
        System,
        Windows,
        Unix,
        Mac
    }
    public class TextStream : Stream, IDisposable
    {
        private readonly Stream _stream;
        private long _distanceFromEnd;

        
        public bool FileOpen { get; private set; }
        public TextStream(Stream stream)
        {
            _stream = stream;
            FileOpen = true;
            _distanceFromEnd = _stream.Length;
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
        //Opens the file in the specified mode
        //public void Open(string path, FileMode openMode)
        //{
        //    _stream = File.Open(path, openMode);
        //    _distanceFromEnd = _stream.Length;
        //    FileOpen = true;
        //}

        //Reads the next char from a stream Returns a null character is none left
        public char ReadChar()
        {
            if (_stream == null) throw new ArgumentException("File must be opened first");
            if (_distanceFromEnd <= 0) return '\0';
            var bytesToRead = 0;
            
            var line = new byte[5]; // UTF-8 max bytes for a char is around 4
             _ = Read(line, 0, 1);
             --_distanceFromEnd;
             var bits = new BitArray(line);
             for (var i = 7; i >= 3; --i) // Only read first few to find out size
             {
                 if (bits[i])
                 {
                     ++bytesToRead;
                 }
                 else
                 {
                     break;
                 }
             }

             for (var i = 1; bytesToRead - 1 > 0; --bytesToRead, i++)
             {
                 _=Read(line, i, 1);
                 --_distanceFromEnd;
             }

             var result = Encoding.UTF8.GetString(line);
             return result[0];
        }

        
        public string ReadNumChars(long numberOfCharacters)
        {
            var currentCharacter = '0';
            var result = new StringBuilder();

            for (long numberRead = 0;
                currentCharacter != char.MinValue && numberRead <= numberOfCharacters;
                ++numberRead)
            {
                currentCharacter = ReadChar();
                if (currentCharacter != char.MinValue) 
                    result.Append(currentCharacter);
            }

            return result.ToString();
        }
        
        //Reads to the Next Instance of a Delimiter or the end of the file.
        public string ReadToDelimiter(char delimiter)
        {
            var currentCharacter = '0';
            var result = new StringBuilder();
            var split = false;
            
            for (; !split && currentCharacter != '\0';)
            {
                currentCharacter = ReadChar();
                if (currentCharacter == delimiter) split = true;
                else
                {
                    result.Append(currentCharacter);
                }
            }
            return result.ToString();
        }
        
        public void Ignore()
        {
            _= ReadChar();
        }
        public void Ignore(long numberOfCharactersToIgnore)
        {
            var currentCharacter = '0';

            for (long numberRead = 0; currentCharacter != char.MinValue && numberRead <= numberOfCharactersToIgnore; ++numberRead)
            {
                currentCharacter = ReadChar();
            }
        }
        
        public void Ignore(long numberOfCharactersToIgnore, char delimiter)
        {
            var currentCharacter = '0';
            var split = false;
            
            for (long numberRead = 0; !split && currentCharacter != char.MinValue && numberRead <= numberOfCharactersToIgnore; ++numberRead)
            {
                currentCharacter = ReadChar();
                if (currentCharacter == delimiter) split = true;
            }
        }

        // Reads until the next new line, works for \n & \r\n
        public string ReadLine()
        {
            if (_stream == null) throw new ArgumentException("File must be opened first");
            var buffer = new byte[1]; // Only reading one byte at a time, so no need for a larger buffer
            var split = false;
            var ms = new MemoryStream();
            var result = string.Empty;
            byte currentByte = 1;

            for (; !split && _distanceFromEnd != 0;)
            {
                _ = Read(buffer, 0, 1);
                var previousByte = currentByte;
                currentByte = buffer[0];
                --_distanceFromEnd;
                ms.Write(buffer, 0, 1); // Takes care of storing the bytes read in memory

                if (currentByte != (byte) '\n') continue;

                var line = ms.ToArray();
                //Array.Resize(ref buffer, currentSize);

                if (previousByte == (byte) '\r')
                {
                    Array.Resize(ref line, line.Length - 1); // Removes one
                }

                Array.Resize(ref line, line.Length - 1); // removes \n if \r does not exists
                result = Encoding.UTF8.GetString(line, 0, line.Length);
                split = true;
            }
            return result;
        }

        public string ReadAllLines()
        {
            if (_stream == null) throw new ArgumentException("File must be opened first");
            var buffer = new byte[256]; // Reading in larger chunks
            var ms = new MemoryStream();
            int lengthOfBuffer;
            while ((lengthOfBuffer = Read(buffer, 0, buffer.Length)) > 0)
                ms.Write(buffer,0,lengthOfBuffer);
            var result = Encoding.UTF8.GetString(ms.ToArray(), 0, (int) ms.Length);
            return result;
        }
        public void IgnoreLine()
        {
            _ = ReadLine();
        }

        // returns next character without moving pointer, returns null character is next is end of the file
        public char Peek()
        {
            var position = Position;
            var peeked = ReadChar();
            var bytes = Encoding.UTF8.GetBytes(peeked.ToString());
            _distanceFromEnd += bytes.Length;
            Position = position;
            return peeked;
        }

        //Writes an entire string to a text file
        public void WriteString(string stringToWrite)
        {
            if (_stream == null) throw new ArgumentException("File must be opened first");
            var byteString = new List<byte>();
            foreach (var character in stringToWrite)
            {
                var chrBytes = Encoding.UTF8.GetBytes(character.ToString());
                foreach (var cbyte in chrBytes)
                {
                    byteString.Add(cbyte);
                }
            }
            
            Write(byteString.ToArray());
            Flush();
        }

        //Writes a string but attaches a newline to the end of it.
        public void WriteLine(string lineToWrite)
        {
            if (_stream == null) throw new ArgumentException("File must be opened first");
            var byteString = lineToWrite.SelectMany(character => Encoding.UTF8.GetBytes(character.ToString())).ToList();

            var newLine = Environment.NewLine;
            
            byteString.AddRange(newLine.Select(nl => (byte) nl));

            Write(byteString.ToArray());
            Flush();
        }
        
        public void WriteLine(string lineToWrite, NewLine newLineType)
        {
            if (_stream == null) throw new ArgumentException("File must be opened first");
            var byteString = lineToWrite.SelectMany(character => Encoding.UTF8.GetBytes(character.ToString())).ToList();

            var newLine = newLineType switch
            {
                NewLine.System => Environment.NewLine,
                NewLine.Windows => "\r\n",
                NewLine.Unix => "\n",
                NewLine.Mac => "\r",
                _ => throw new ArgumentException("Invalid New Line")
            };
            byteString.AddRange(newLine.Select(nl => (byte) nl));

            Write(byteString.ToArray());
            Flush();
        }

        public new void Dispose()
        {
            _stream?.Dispose();
            FileOpen = false;
        }
        
    }
}