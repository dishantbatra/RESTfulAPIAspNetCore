using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public class ArraryModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            //Our Model Binder Works On enumerable Types
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            //Get Input Value Through Value Provider
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            //If value is null or white space we return nulll
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            //The Value is null or white space
            //and the type of the model is enumerable
            //get the enumerable type and convertor
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var convertor = TypeDescriptor.GetConverter(elementType);

            //Convert each item in the value list to the ienumerable type
            var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            //Create an array of that type,and set it as model value
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            //return a succesful result passing in the model
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;

        }
    }
}
