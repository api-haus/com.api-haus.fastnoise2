namespace FastNoise2.NativeTexture
{
   using System.Runtime.InteropServices;

   [StructLayout(LayoutKind.Sequential)]
   public struct ValueBounds<TValue> where TValue : unmanaged
   {
	  public TValue Min;
	  public TValue Max;
	  public TValue Scale;

	  public ValueBounds(TValue min, TValue max, TValue scale = default)
	  {
		 Min = min;
		 Max = max;
		 Scale = scale;
	  }
   }
}
