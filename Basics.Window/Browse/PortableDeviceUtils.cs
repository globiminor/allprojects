
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Basics.Window.Browse
{
  static class PortableDeviceUtils
  {
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct PropVariant
    {
      [FieldOffset(0)]
      public short variantType;
      [FieldOffset(8)]
      public IntPtr pointerValue;
      [FieldOffset(8)]
      public byte byteValue;
      [FieldOffset(8)]
      public long longValue;
    }

    public static IEnumerable<string> GetDevices()
    {
      PortableDeviceApiLib.PortableDeviceManager devMgr = new PortableDeviceApiLib.PortableDeviceManager();
      PortableDeviceApiLib.IPortableDeviceManager iDevMgr = devMgr;
      uint nDev = 0;
      devMgr.RefreshDeviceList();

      devMgr.GetDevices(null, ref nDev);
      string[] devices = new string[nDev];
      devMgr.GetDevices(devices, ref nDev);

      for (int i = 0; i < nDev; i++)
      {
        uint maxChar = 1024;
        ushort[] t = new ushort[maxChar];

        uint nChar = maxChar;
        devMgr.GetDeviceFriendlyName(devices[i], ref t[0], ref nChar);
        string device = GetString(t, nChar);

        yield return device;
      }

      System.Runtime.InteropServices.Marshal.ReleaseComObject(devMgr);
    }

    public static IEnumerable<string> GetContent(IList<string> dirs)
    {
      PortableDeviceApiLib.PortableDeviceManager devMgr = new PortableDeviceApiLib.PortableDeviceManager();
      PortableDeviceApiLib.IPortableDeviceManager iDevMgr = devMgr;
      uint nDev = 0;
      devMgr.RefreshDeviceList();

      devMgr.GetDevices(null, ref nDev);
      string[] devices = new string[nDev];
      devMgr.GetDevices(devices, ref nDev);

      for (int i = 0; i < nDev; i++)
      {
        uint maxChar = 1024;
        ushort[] t = new ushort[maxChar];

        uint nChar = maxChar;
        devMgr.GetDeviceFriendlyName(devices[i], ref t[0], ref nChar);
        string device = GetString(t, nChar);

        if (device.Equals(dirs[0]))
        {
          PortableDeviceApiLib.IPortableDeviceValues devVals = InitDeviceValues();
          PortableDeviceApiLib.PortableDeviceFTM dev = new PortableDeviceApiLib.PortableDeviceFTM();

          dev.Open(devices[i], devVals);

          PortableDeviceApiLib.IPortableDeviceContent content;
          dev.Content(out content);

          foreach (string entry in EnumRecursive(content, "DEVICE", dirs, 1))
          {
            yield return entry;
          }

          Marshal.ReleaseComObject(devVals);
        }
      }

      Marshal.ReleaseComObject(devMgr);
    }

    private static IEnumerable<string> EnumRecursive(PortableDeviceApiLib.IPortableDeviceContent content, 
      string parentId, IList<string> dirs, int pos)
    {
      PortableDeviceApiLib.IEnumPortableDeviceObjectIDs enumObjIds;
      content.EnumObjects(0, parentId, null, out enumObjIds);

      List<string> ids = new List<string>();
      string objId;
      uint fetched = 1;
      while (fetched > 0)
      {
        enumObjIds.Next(1, out objId, ref fetched);
        if (fetched > 0)
        { ids.Add(objId); }
      }

      foreach (string id in ids)
      {
        PortableDeviceApiLib.IPortableDeviceProperties props;
        content.Properties(out props);

        PortableDeviceApiLib.IPortableDeviceValues values;
        props.GetValues(id, null, out values);

        uint nValues = 0;
        values.GetCount(ref nValues);
        for (uint iValue = 0; iValue < nValues; iValue++)
        {
          PortableDeviceApiLib._tagpropertykey propKey =
                  new PortableDeviceApiLib._tagpropertykey();
          PortableDeviceApiLib.tag_inner_PROPVARIANT ipValue =
                          new PortableDeviceApiLib.tag_inner_PROPVARIANT();
          values.GetAt(iValue, ref propKey, ref ipValue);

          if (propKey.pid != 4)
          {
            continue;
          }
          //
          // Allocate memory for the intermediate marshalled object
          // and marshal it as a pointer
          //
          IntPtr ptrValue = Marshal.AllocHGlobal(Marshal.SizeOf(ipValue));
          Marshal.StructureToPtr(ipValue, ptrValue, false);

          //
          // Marshal the pointer into our C# object
          //
          PropVariant pvValue =
              (PropVariant)Marshal.PtrToStructure(ptrValue, typeof(PropVariant));

          //
          // Display the property if it a string (VT_LPWSTR is decimal 31)
          //
          if (pvValue.variantType == 31 /*VT_LPWSTR*/)
          {
            string value = Marshal.PtrToStringUni(pvValue.pointerValue);
            if (pos >= dirs.Count)
            {
              yield return value;
            }
            else if (value == dirs[pos])
            {
              foreach (string entry in EnumRecursive(content, id, dirs, pos + 1))
              {
                yield return entry;
              }
            }
          }
        }
      }
    }

    private static PortableDeviceApiLib.IPortableDeviceValues InitDeviceValues()
    {
      // We'll use an IPortableDeviceValues object to transform the
      // string into a PROPVARIANT
      PortableDeviceApiLib.IPortableDeviceValues pValues =
          (PortableDeviceApiLib.IPortableDeviceValues)
              new PortableDeviceTypesLib.PortableDeviceValues();

      return pValues;

      //// We insert the string value into the IPortableDeviceValues object
      //// using the SetStringValue method
      //pValues.SetStringValue(ref PortableDevicePKeys.WPD_OBJECT_ID, value);

      //// We then extract the string into a PROPVARIANT by using the 
      //// GetValue method
      //pValues.GetValue(ref PortableDevicePKeys.WPD_OBJECT_ID,
      //                            out propvarValue);

    }


    private static string GetString(ushort[] t, uint count)
    {
      char[] charArray = new char[count - 1];
      System.Array.Copy(t, charArray, count - 1);
      string s = new string(charArray);
      return s;
    }

  }
}
