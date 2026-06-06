using Microsoft.VisualStudio.TestTools.UnitTesting;

// These tests bind real TCP ports and stand up overlay listeners; they must not run concurrently.
[assembly: DoNotParallelize]
