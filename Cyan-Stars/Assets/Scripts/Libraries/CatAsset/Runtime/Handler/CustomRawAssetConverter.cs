using System;
using System.Threading.Tasks;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 自定义原生资源转换器的原型
    /// </summary>
    public interface ICustomRawAssetConverter
    {
        /// <summary>
        /// 转换原生资源数据为指定类型的资源对象
        /// </summary>
        Task<object> Convert(byte[] bytes);
    }

    public abstract class BaseCustomRawAssetConverter<T> : ICustomRawAssetConverter
    {
        /// <inheritdoc cref="ICustomRawAssetConverter.Convert"/>
        public abstract Task<T> Convert(byte[] bytes);

        /// <inheritdoc />
        async Task<object> ICustomRawAssetConverter.Convert(byte[] bytes)
        {
            return await Convert(bytes);
        }
    }


    public delegate T CustomRawAssetConverterFunc<T>(byte[] bytes);

    public delegate
#if !UNITASK
        Task<T>
#else
        UniTask<T>
#endif
        AsyncCustomRawAssetConverterFunc<T>(byte[] bytes);

    public sealed class AnymousCustomRawAssetConverter<T> : ICustomRawAssetConverter
    {
        private readonly object convert;

        public AnymousCustomRawAssetConverter(AsyncCustomRawAssetConverterFunc<T> convert)
        {
            this.convert = convert;
        }

        public AnymousCustomRawAssetConverter(CustomRawAssetConverterFunc<T> convert)
        {
            this.convert = convert;
        }

        /// <inheritdoc />
        public async Task<object> Convert(byte[] bytes)
        {
            if (convert is AsyncCustomRawAssetConverterFunc<T> asyncConverter)
            {
                return await asyncConverter(bytes);
            }
            else if (convert is CustomRawAssetConverterFunc<T> converter)
            {
                return converter(bytes);
            }

            if (convert is null)
            {
                throw new NullReferenceException("Converter is null");
            }

            throw new InvalidOperationException("Invalid converter type");
        }
    }
}
