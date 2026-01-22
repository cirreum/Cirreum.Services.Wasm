namespace Cirreum.FileSystem;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

sealed class NotImplementedFileSystem : IFileSystem {

	void IFileSystem.CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite) {
		throw new NotImplementedException();
	}

	Task IFileSystem.CopyDirectoryAsync(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	void IFileSystem.CopyFile(string sourceFileName, string destFileName, bool overwrite) {
		throw new NotImplementedException();
	}

	Task IFileSystem.CopyFileAsync(string sourceFileName, string destFileName, bool overwrite, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	void IFileSystem.DeleteChildDirectories(string rootPath) {
		throw new NotImplementedException();
	}

	Task IFileSystem.DeleteChildDirectoriesAsync(string rootPath, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	void IFileSystem.DeleteDirectory(string path, bool recursive) {
		throw new NotImplementedException();
	}

	Task IFileSystem.DeleteDirectoryAsync(string path, bool recursive, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	void IFileSystem.DeleteFile(string path) {
		throw new NotImplementedException();
	}

	Task IFileSystem.DeleteFileAsync(string path, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	void IFileSystem.DeleteFileWithRetry(string path) {
		throw new NotImplementedException();
	}

	Task IFileSystem.DeleteFileWithRetryAsync(string path, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	bool IFileSystem.DirectoryExists(string path) {
		throw new NotImplementedException();
	}

	bool IFileSystem.EnsureDirectory(string path) {
		throw new NotImplementedException();
	}

	void IFileSystem.ExtractZipFile(string source, string destination, bool overwriteFiles) {
		throw new NotImplementedException();
	}

	Task IFileSystem.ExtractZipFileAsync(string source, string destination, bool overwriteFiles, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	bool IFileSystem.FileExists(string path) {
		throw new NotImplementedException();
	}

	string[] IFileSystem.GetFiles(string path, string searchPattern, bool includeChildDirectories) {
		throw new NotImplementedException();
	}

	void IFileSystem.MoveDirectory(string sourceDirName, string destDirName) {
		throw new NotImplementedException();
	}

	Task IFileSystem.MoveDirectoryAsync(string sourceDirName, string destDirName, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	void IFileSystem.MoveFile(string sourceFileName, string destFileName, bool overwrite) {
		throw new NotImplementedException();
	}

	Task IFileSystem.MoveFileAsync(string sourceFileName, string destFileName, bool overwrite, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	IEnumerable<string> IFileSystem.QueryDirectories(string[] paths, bool includeChildDirectories, string searchPattern, Func<string, bool>? predicate, int take) {
		throw new NotImplementedException();
	}

	IEnumerable<string> IFileSystem.QueryDirectories(string path, bool includeChildDirectories, string searchPattern, Func<string, bool>? predicate, int take) {
		throw new NotImplementedException();
	}

	IEnumerable<string> IFileSystem.QueryDirectories(string path, bool includeChildDirectories, IEnumerable<string> searchPatterns, Func<string, bool>? predicate, int take) {
		throw new NotImplementedException();
	}

	IAsyncEnumerable<string> IFileSystem.QueryDirectoriesAsync(string[] paths, bool includeChildDirectories, string searchPattern, Func<string, bool>? predicate, int take, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	IAsyncEnumerable<string> IFileSystem.QueryDirectoriesAsync(string path, bool includeChildDirectories, string searchPattern, Func<string, bool>? predicate, int take, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	IAsyncEnumerable<string> IFileSystem.QueryDirectoriesAsync(string path, bool includeChildDirectories, IEnumerable<string> searchPatterns, Func<string, bool>? predicate, int take, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	IEnumerable<string> IFileSystem.QueryFiles(string[] paths, bool includeChildDirectories, string searchPattern, Func<string, bool>? predicate, int take) {
		throw new NotImplementedException();
	}

	IEnumerable<string> IFileSystem.QueryFiles(string path, bool includeChildDirectories, string searchPattern, Func<string, bool>? predicate, int take) {
		throw new NotImplementedException();
	}

	IEnumerable<string> IFileSystem.QueryFiles(string path, bool includeChildDirectories, IEnumerable<string> searchPatterns, Func<string, bool>? predicate, int take) {
		throw new NotImplementedException();
	}

	IAsyncEnumerable<string> IFileSystem.QueryFilesAsync(string[] paths, bool includeChildDirectories, string searchPattern, Func<string, bool>? predicate, int take, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	IAsyncEnumerable<string> IFileSystem.QueryFilesAsync(string path, bool includeChildDirectories, string searchPattern, Func<string, bool>? predicate, int take, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	IAsyncEnumerable<string> IFileSystem.QueryFilesAsync(string path, bool includeChildDirectories, IEnumerable<string> searchPatterns, Func<string, bool>? predicate, int take, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	string IFileSystem.ReadAllText(string path) {
		throw new NotImplementedException();
	}

	Task<string> IFileSystem.ReadAllTextAsync(string path, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

	void IFileSystem.WriteAllText(string path, string contents) {
		throw new NotImplementedException();
	}

	Task IFileSystem.WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}

}