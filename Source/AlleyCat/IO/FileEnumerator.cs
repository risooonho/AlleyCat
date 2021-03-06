using System.Collections;
using System.Collections.Generic;
using AlleyCat.Common;
using EnsureThat;
using Godot;
using Microsoft.Extensions.FileProviders;

namespace AlleyCat.IO
{
    public class FileEnumerator : IEnumerator<IFileInfo>
    {
        private const string CurrentDir = ".";

        private const string ParentDir = "..";

        public IFileInfo Current { get; private set; }

        object IEnumerator.Current => Current;

        private readonly Directory _directory;

        private readonly string _path;

        private readonly bool _skipNavigational;

        private readonly bool _skipHidden;

        private readonly bool _endsWithSeparator;

        public FileEnumerator(
            string path, bool skipNavigational = false, bool skipHidden = false)
        {
            Ensure.That(path, nameof(path)).IsNotNull();

            _path = path;
            _skipNavigational = skipNavigational;
            _skipHidden = skipHidden;

            _endsWithSeparator = _path.EndsWith(FileInfo.Separator);

            _directory = new Directory();

            try
            {
                _directory.Open(path).ThrowOnError(e => $"Failed to open directory: '{path}' ({e}).");
            }
            catch
            {
                _directory.Dispose();
                throw;
            }

            _directory.ListDirBegin(skipNavigational, skipHidden);
        }

        public bool MoveNext()
        {
            var path = _directory.GetNext();

            while (path == CurrentDir || path == ParentDir)
            {
                path = _directory.GetNext();
            }

            var hasNext = !string.IsNullOrEmpty(path);

            if (hasNext)
            {
                var absolute = string.Join(_endsWithSeparator ? "" : "/", _path, path);

                if (_directory.CurrentIsDir())
                {
                    Current = new DirectoryInfo(absolute);
                }
                else
                {
                    Current = new FileInfo(absolute);
                }
            }
            else
            {
                Current = null;
            }

            return hasNext;
        }

        public void Reset()
        {
            _directory.ListDirEnd();
            _directory.ListDirBegin(_skipNavigational, _skipHidden);
        }

        public void Dispose()
        {
            _directory.ListDirEnd();
            _directory.Dispose();
        }
    }
}
