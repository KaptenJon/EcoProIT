﻿/*
Copyright (c) 2010 Stephen P Ward and Joseph E Feser

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
using System.Windows.Data;
using System;
using System.Windows.Controls;

namespace FeserWard.Controls {
    internal static class CloneHelper {

        public static Binding Clone(Binding original) {
            if (original == null)
                throw new ArgumentNullException("original");

            var bind = new Binding() {
                BindingGroupName = original.BindingGroupName,
                BindsDirectlyToSource = original.BindsDirectlyToSource,
                Converter = original.Converter,
                ConverterCulture = original.ConverterCulture,
                ConverterParameter = original.ConverterParameter,
                FallbackValue = original.FallbackValue,
                IsAsync = original.IsAsync,
                Mode = original.Mode,
                NotifyOnSourceUpdated = original.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = original.NotifyOnTargetUpdated,
                NotifyOnValidationError = original.NotifyOnValidationError,
                Path = original.Path,
                StringFormat = original.StringFormat,
                TargetNullValue = original.TargetNullValue,
                UpdateSourceExceptionFilter = original.UpdateSourceExceptionFilter,
                UpdateSourceTrigger = original.UpdateSourceTrigger,
                ValidatesOnDataErrors = original.ValidatesOnDataErrors,
                ValidatesOnExceptions = original.ValidatesOnExceptions,
                XPath = original.XPath

            };

            if (!string.IsNullOrEmpty(original.ElementName))
                bind.ElementName = original.ElementName;

            if (original.RelativeSource != null)
                bind.RelativeSource = original.RelativeSource;

            if (original.Source != null)
                bind.Source = original.Source;

            for (int i = 0; i < original.ValidationRules.Count; i++) {
                bind.ValidationRules.Add(original.ValidationRules[i]);
            }

            return bind;
        }

        public static BindingBase Clone(BindingBase original) {
            if (original is Binding) {
                return Clone(original as Binding);
            }
            else if (original is MultiBinding) {
                return Clone(original as MultiBinding);
            }
            else if (original is PriorityBinding) {
                return Clone(original as PriorityBinding);
            }

            throw new ArgumentOutOfRangeException("original", "Unrecognized type: " + original.GetType().ToString());
        }

        public static GridViewColumn Clone(IntelliboxColumn original) {
            if (original == null)
                throw new ArgumentNullException("original");

            var copy = new GridViewColumn() {
                DisplayMemberBinding = original.DisplayMemberBinding
            };

            if (BindingOperations.IsDataBound(original, GridViewColumn.CellTemplateProperty)) {
                var duplBind = Clone(BindingOperations.GetBindingBase(original, GridViewColumn.CellTemplateProperty));
                BindingOperations.SetBinding(copy, GridViewColumn.CellTemplateProperty, duplBind);
            }
            else if (original.CellTemplate != null) {
                copy.CellTemplate = original.CellTemplate;
            }

            if (BindingOperations.IsDataBound(original, GridViewColumn.CellTemplateSelectorProperty)) {
                var duplBind = Clone(BindingOperations.GetBindingBase(original, GridViewColumn.CellTemplateSelectorProperty));
                BindingOperations.SetBinding(copy, GridViewColumn.CellTemplateSelectorProperty, duplBind);
            }
            else if (original.CellTemplateSelector != null) {
                copy.CellTemplateSelector = original.CellTemplateSelector;
            }

            if (BindingOperations.IsDataBound(original, GridViewColumn.HeaderProperty)) {
                var duplBind = Clone(BindingOperations.GetBindingBase(original, GridViewColumn.HeaderProperty));
                BindingOperations.SetBinding(copy, GridViewColumn.HeaderProperty, duplBind);
            }
            else if (original.Header != null) {
                copy.Header = original.Header;
            }

            if (BindingOperations.IsDataBound(original, GridViewColumn.HeaderContainerStyleProperty)) {
                var duplBind = Clone(BindingOperations.GetBindingBase(original, GridViewColumn.HeaderContainerStyleProperty));
                BindingOperations.SetBinding(copy, GridViewColumn.HeaderContainerStyleProperty, duplBind);
            }
            else if (original.HeaderContainerStyle != null) {
                copy.HeaderContainerStyle = original.HeaderContainerStyle;
            }

            if (BindingOperations.IsDataBound(original, GridViewColumn.HeaderStringFormatProperty)) {
                var duplBind = Clone(BindingOperations.GetBindingBase(original, GridViewColumn.HeaderStringFormatProperty));
                BindingOperations.SetBinding(copy, GridViewColumn.HeaderStringFormatProperty, duplBind);
            }
            else if (original.HeaderStringFormat != null) {
                copy.HeaderStringFormat = original.HeaderStringFormat;
            }

            if (BindingOperations.IsDataBound(original, GridViewColumn.HeaderTemplateProperty)) {
                var duplBind = Clone(BindingOperations.GetBindingBase(original, GridViewColumn.HeaderTemplateProperty));
                BindingOperations.SetBinding(copy, GridViewColumn.HeaderTemplateProperty, duplBind);
            }
            else if (original.HeaderTemplate != null) {
                copy.HeaderTemplate = original.HeaderTemplate;
            }

            if (BindingOperations.IsDataBound(original, GridViewColumn.HeaderTemplateSelectorProperty)) {
                var duplBind = Clone(BindingOperations.GetBindingBase(original, GridViewColumn.HeaderTemplateSelectorProperty));
                BindingOperations.SetBinding(copy, GridViewColumn.HeaderTemplateSelectorProperty, duplBind);
            }
            else if (original.HeaderTemplateSelector != null) {
                copy.HeaderTemplateSelector = original.HeaderTemplateSelector;
            }

            if (BindingOperations.IsDataBound(original, GridViewColumn.WidthProperty)) {
                var duplBind = Clone(BindingOperations.GetBindingBase(original, GridViewColumn.WidthProperty));
                BindingOperations.SetBinding(copy, GridViewColumn.WidthProperty, duplBind);
            }
            else {
                copy.Width = original.Width;
            }

            return copy;
        }

        public static MultiBinding Clone(MultiBinding original) {
            if (original == null)
                throw new ArgumentNullException("original");

            var t = original;
            var bind = new MultiBinding() {
                BindingGroupName = t.BindingGroupName,
                Converter = t.Converter,
                ConverterCulture = t.ConverterCulture,
                ConverterParameter = t.ConverterParameter,
                FallbackValue = t.FallbackValue,
                Mode = t.Mode,
                NotifyOnSourceUpdated = t.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = t.NotifyOnTargetUpdated,
                NotifyOnValidationError = t.NotifyOnValidationError,
                StringFormat = t.StringFormat,
                TargetNullValue = t.TargetNullValue,
                UpdateSourceExceptionFilter = t.UpdateSourceExceptionFilter,
                UpdateSourceTrigger = t.UpdateSourceTrigger,
                ValidatesOnDataErrors = t.ValidatesOnDataErrors,
                ValidatesOnExceptions = t.ValidatesOnExceptions
            };

            for (int i = 0; i < t.Bindings.Count; i++) {
                bind.Bindings.Add(Clone(t.Bindings[i]));
            }

            for (int i = 0; i < t.ValidationRules.Count; i++) {
                bind.ValidationRules.Add(t.ValidationRules[i]);
            }
            return bind;
        }

        public static PriorityBinding Clone(PriorityBinding original) {
            if (original == null)
                throw new ArgumentNullException("original");

            var t = original;
            var bind = new PriorityBinding() {
                BindingGroupName = t.BindingGroupName,
                FallbackValue = t.FallbackValue,
                StringFormat = t.StringFormat,
                TargetNullValue = t.TargetNullValue
            };

            for (int i = 0; i < t.Bindings.Count; i++) {
                bind.Bindings.Add(Clone(t.Bindings[i]));
            }

            return bind;
        }
    }
}
