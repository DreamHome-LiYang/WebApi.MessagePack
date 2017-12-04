using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using MsgPack;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using MsgPack.Serialization;

namespace WebApi.MessagePack.App_Start
{
    public class MessagePackFormatter : MediaTypeFormatter
    {
        Func<Type, bool> IsAllowedType = (t) =>
        {
            if (!t.IsAbstract && !t.IsInterface && t != null && !t.IsNotPublic)
                return true;
            if (typeof(IEnumerable).IsAssignableFrom(t))
                return true;
            return false;
        };
        public MessagePackFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/x-msgpack"));
        }
        public override bool CanWriteType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("Type is null");
            return IsAllowedType(type);
        }
        public override Task WriteToStreamAsync(Type type, object value, System.IO.Stream stream, HttpContent content, TransportContext transportContext)
        {
            if (type == null)
                throw new ArgumentNullException("type is null");
            if (stream == null)
                throw new ArgumentNullException("Write stream is null");
            var tcs = new TaskCompletionSource<object>();
            if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            {
                value = (value as IEnumerable<object>).ToList();
            }
            var serializer = MessagePackSerializer.Get<dynamic>();
            serializer.Pack(stream, value);
            tcs.SetResult(null);
            return tcs.Task;
        }
        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var tcs = new TaskCompletionSource<object>();
            if (content.Headers != null && content.Headers.ContentLength == 0)
                return null;
            try
            {
                var serializer = MessagePackSerializer.Get(type);
                object result;
                using (var mpUnpacker = Unpacker.Create(stream))
                {
                    mpUnpacker.Read();
                    result = serializer.UnpackFrom(mpUnpacker);
                }
                tcs.SetResult(result);
            }
            catch (Exception e)
            {
                if (formatterLogger == null) throw;
                formatterLogger.LogError(String.Empty, e.Message);
                tcs.SetResult(GetDefaultValueForType(type));
            }
            return tcs.Task;
        }
        public override bool CanReadType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type is null");
            return IsAllowedType(type);
        }
    }
}
