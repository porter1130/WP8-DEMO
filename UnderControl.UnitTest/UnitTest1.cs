using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCController.Input;
using UnderControl.Communication;

namespace UnderControl.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Win32Wrapper.SetCursorPosition(12, 13);
            Win32Wrapper.SetCursorPosition(50,55 );
            Win32Wrapper.SetCursorPosition(1375, 919);
            Win32Wrapper.SetCursorPosition(220, 130);
            
        }
    }
}
