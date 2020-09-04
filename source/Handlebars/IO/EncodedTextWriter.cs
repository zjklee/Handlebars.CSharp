using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace HandlebarsDotNet
{
	public sealed class EncodedTextWriter : TextWriter
	{
		private static readonly EncodedTextWriterPool Pool = new EncodedTextWriterPool();
		
		private ITextEncoder _encoder;

		public bool SuppressEncoding { get; set; }

		private EncodedTextWriter()
		{
			
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static EncodedTextWriter From(TextWriter writer, ITextEncoder encoder)
		{
			if (writer is EncodedTextWriter encodedTextWriter) return encodedTextWriter;
			
			var textWriter = Pool.Get();
			textWriter._encoder = encoder;
			textWriter.UnderlyingWriter = writer;

			return textWriter;
		}

		public void Write(string value, bool encode)
		{
			if(encode && !SuppressEncoding && (_encoder != null))
			{
				value = _encoder.Encode(value);
			}

			UnderlyingWriter.Write(value);
		}

		public override void Write(string value) => Write(value, true);

		public override void Write(char value)
		{
			if (_encoder.RequireEncoding(value))
			{
				Write(value.ToString(), true);
				return;
			}
			
			UnderlyingWriter.Write(value);
		}

		public override void Write(object value)
		{
			switch (value)
			{
				case null:
					return;
				
				case ISafeString safeString:
					Write(safeString.Value, false);
					safeString.Dispose();
					return;
				
				case string str when !string.IsNullOrEmpty(str):
					Write(str, true);
					return;
				
				case string _:
					return;

				default:
					var s = value.ToString();
					if(string.IsNullOrEmpty(s)) return;
			
					Write(s, true);
					return;
			}
		}

		public TextWriter UnderlyingWriter { get; private set; }

		protected override void Dispose(bool disposing)
		{
			Pool.Return(this);
		}

		public override Encoding Encoding => UnderlyingWriter.Encoding;
		
		private class EncodedTextWriterPool : InternalObjectPool<EncodedTextWriter>
		{
			public EncodedTextWriterPool() : base(new Policy())
			{
			}
			
			private class Policy : IInternalObjectPoolPolicy<EncodedTextWriter>
			{
				public EncodedTextWriter Create()
				{
					return new EncodedTextWriter();
				}

				public bool Return(EncodedTextWriter obj)
				{
					obj._encoder = null;
					obj.UnderlyingWriter = null;
					
					return true;
				}
			}
		}
	}
}