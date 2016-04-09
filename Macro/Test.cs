using System.Windows.Forms;
using System;

namespace Macro
{
  class Test
  {
    byte[] lastbCharData = new byte[256];
    protected char GetAsciiCharacter(int iKeyCode, int iFlags)
    {
      Macro macro = new Macro();
      uint lpChar = 0;

      byte[] bCharData = new byte[256];

      Macro.GetKeyboardState(bCharData);

      for (int i = 0; i < 256; i++)
      {
        Console.Write(string.Format("{0,3} ", bCharData[i]));
      }
      Console.WriteLine();
      Console.WriteLine(string.Format("{0} {1} ", iKeyCode, iFlags));

      Macro.ToAscii(iKeyCode, iFlags, bCharData, ref lpChar, 1);
      //ToAscii(66, 66, bCharData, ref lpChar, 1);
      for (int i = 0; i < 256; i++)
      {
        lastbCharData[i] = bCharData[i];
      }
      return (char)lpChar;
    }

    private void txtHandle_KeyDown(object sender, KeyEventArgs e)
    {
      Console.WriteLine("Key Pressed GetAscii: " +
GetAsciiCharacter((int)e.KeyCode, e.KeyValue));

    }

  }
}
