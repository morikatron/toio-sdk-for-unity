namespace toio.VisualScript
{
    public static class ConverterToByte
    {
        public static byte EnumToByte(Cube.NOTE_NUMBER input_enum)
        {
            return (byte)input_enum;
        }

        public static byte FloatToByte(float input_float)
        {
            return (byte)input_float;
        }
    }
}