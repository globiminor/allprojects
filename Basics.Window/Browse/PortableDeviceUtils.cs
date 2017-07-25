
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Basics.Window.Browse
{
  class PdEntry
  {
    public string Id { get; set; }
    public string Name { get; set; }
  }

  public static class PortableDeviceUtils
  {
    private class DevMgr : IDisposable
    {
      private PortableDeviceApiLib.PortableDeviceManager _devMgr;
      public DevMgr()
      {
        _devMgr = new PortableDeviceApiLib.PortableDeviceManager();
      }
      public PortableDeviceApiLib.PortableDeviceManager Base { get { return _devMgr; } }
      public void Dispose()
      {
        if (_devMgr != null)
        { Marshal.ReleaseComObject(_devMgr); }
        _devMgr = null;
      }
    }

    private class DevContent : IDisposable
    {
      private PortableDeviceApiLib.IPortableDeviceValues _devVals;
      private PortableDeviceApiLib.IPortableDeviceContent _content;
      public DevContent(string deviceId)
      {
        _devVals = InitDeviceValues();
        PortableDeviceApiLib.PortableDeviceFTM dev = new PortableDeviceApiLib.PortableDeviceFTM();

        dev.Open(deviceId, _devVals);

        dev.Content(out _content);
      }

      public PortableDeviceApiLib.IPortableDeviceContent Base { get { return _content; } }
      public void Dispose()
      {
        if (_devVals != null)
        {
          Marshal.ReleaseComObject(_devVals);
        }
        _devVals = null;
      }
    }

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
      using (DevMgr mgr = new DevMgr())
      {
        foreach (string deviceId in GetDeviceIds(mgr.Base))
        {
          yield return GetDeviceFriendlyName(mgr.Base, deviceId);
        }
      }
    }

    public static void Deserialize<T>(string path, out T obj)
    {
      if (!File.Exists(path))
      {
        using (Stream s = new MemoryStream())
        {
          Read(path, s);

          s.Seek(0, SeekOrigin.Begin);

          using (TextReader r = new StreamReader(s))
          { Serializer.Deserialize(out obj, r); }
        }
      }
      else
      {
        using (TextReader r = new StreamReader(path))
        { Serializer.Deserialize(out obj, r); }
      }
    }
    public static void Read(string path, Stream stream)
    {
      IList<string> dirs = path.Split(Path.DirectorySeparatorChar);

      using (DevMgr mgr = new DevMgr())
      {
        foreach (string deviceId in GetDeviceIds(mgr.Base))
        {
          string device = GetDeviceFriendlyName(mgr.Base, deviceId);
          if (!device.Equals(dirs[0]))
          { continue; }

          using (DevContent dev = new DevContent(deviceId))
          {
            PdEntry fileEntry = GetEntry(dev.Base, "DEVICE", dirs, 1);

            ReadContent(dev.Base, fileEntry.Id, stream);
          }
        }
      }
    }
    internal static IEnumerable<PdEntry> GetContent(IList<string> dirs)
    {
      using (DevMgr mgr = new DevMgr())
      {
        foreach (string deviceId in GetDeviceIds(mgr.Base))
        {
          string device = GetDeviceFriendlyName(mgr.Base, deviceId);
          if (!device.Equals(dirs[0]))
          { continue; }

          using (DevContent dev = new DevContent(deviceId))
          {
            PdEntry parent = GetEntry(dev.Base, "DEVICE", dirs, 1);
            foreach (PdEntry entry in GetContent(dev.Base, parent.Id))
            {
              yield return entry;
            }
          }
        }
      }
    }

    private static IEnumerable<string> GetDeviceIds(PortableDeviceApiLib.PortableDeviceManager devMgr)
    {
      uint nDev = 0;
      devMgr.RefreshDeviceList();

      devMgr.GetDevices(null, ref nDev);
      string[] devices = new string[nDev];
      devMgr.GetDevices(devices, ref nDev);

      return devices;
    }

    private static string GetDeviceFriendlyName(PortableDeviceApiLib.PortableDeviceManager devMgr, string deviceId)
    {
      uint maxChar = 1024;
      ushort[] t = new ushort[maxChar];

      uint nChar = maxChar;
      devMgr.GetDeviceFriendlyName(deviceId, ref t[0], ref nChar);
      string device = GetString(t, nChar);
      return device;
    }

    public static void TransferContent(string device, string fileId, Stream target)
    {
      using (DevMgr mgr = new DevMgr())
      {
        foreach (string deviceId in GetDeviceIds(mgr.Base))
        {
          string deviceName = GetDeviceFriendlyName(mgr.Base, deviceId);
          if (!device.Equals(deviceName))
          { continue; }

          using (DevContent dev = new DevContent(deviceId))
          {
            ReadContent(dev.Base, fileId, target);
          }
        }
      }
    }

    private static void ReadContent(PortableDeviceApiLib.IPortableDeviceContent content, string fileId,
      Stream target)
    {
      PortableDeviceApiLib.IPortableDeviceResources resources;
      content.Transfer(out resources);

      PortableDeviceApiLib.IStream wpdStream;
      uint optimalTransferSize = 0;
      var property = new PortableDeviceApiLib._tagpropertykey();
      property.fmtid = new Guid(0xE81E79BE, 0x34F0, 0x41BF, 0xB5, 0x3F,
                                0xF1, 0xA0, 0x6A, 0xE8, 0x78, 0x42);
      property.pid = 0;
      resources.GetStream(fileId, ref property, 0, ref optimalTransferSize,
                          out wpdStream);

      System.Runtime.InteropServices.ComTypes.IStream sourceStream =
          (System.Runtime.InteropServices.ComTypes.IStream)wpdStream;

      unsafe
      {
        var buffer = new byte[1024];
        int bytesRead;

        do
        {
          sourceStream.Read(buffer, 1024, new IntPtr(&bytesRead));
          target.Write(buffer, 0, bytesRead);
        } while (bytesRead > 0);
      }
      Marshal.ReleaseComObject(wpdStream);
    }

    private static PdEntry GetEntry(PortableDeviceApiLib.IPortableDeviceContent content,
      string parentId, IList<string> dirs, int pos)
    {
      if (pos == dirs.Count)
      { return new PdEntry { Id = parentId, Name = "" }; }
      List<string> ids = GetObjectIds(content, parentId);

      foreach (string id in ids)
      {
        PdEntry idEntry = GetEntry(content, id);
        if (idEntry == null)
        { continue; }

        if (idEntry.Name == dirs[pos])
        {
          if (pos == dirs.Count - 1)
          { return idEntry; }
          return GetEntry(content, idEntry.Id, dirs, pos + 1);
        }
      }

      return null;
    }

    private static List<string> GetObjectIds(PortableDeviceApiLib.IPortableDeviceContent content,
      string parentId)
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
      return ids;
    }

    private static IEnumerable<PdEntry> GetContent(PortableDeviceApiLib.IPortableDeviceContent content,
      string parentId)
    {
      List<string> ids = GetObjectIds(content, parentId);

      foreach (string id in ids)
      {
        yield return GetEntry(content, id);
      }
    }

    private static PdEntry GetEntry(PortableDeviceApiLib.IPortableDeviceContent content, string id)
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
          return new PdEntry { Id = id, Name = value };
        }
      }
      return null;
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
