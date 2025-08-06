using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TurkSoft.TagHelpers
{
  [HtmlTargetElement("app-input", Attributes = ForAttributeName)]
  public class AppInputTagHelper : TagHelper
  {
    private const string ForAttributeName = "asp-for";
    private const string ItemsAttributeName = "asp-items";
    private const string PlaceholderAttribute = "placeholder";
    private const string LabelAttribute = "asp-label";
    private const string WrapLabelAttribute = "asp-wrap-label";
    private const string ValueAttribute = "value";

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

    [HtmlAttributeName(ItemsAttributeName)]
    public IEnumerable<SelectListItem>? Items { get; set; }

    [HtmlAttributeName(PlaceholderAttribute)]
    public string? Placeholder { get; set; }

    [HtmlAttributeName(LabelAttribute)]
    public string? Label { get; set; }

    [HtmlAttributeName(WrapLabelAttribute)]
    public bool WrapLabel { get; set; } = false;

    [HtmlAttributeName("required")]
    public bool? Required { get; set; } = true;

    [HtmlAttributeName(ValueAttribute)]
    public string? ValueOverride { get; set; }

    [HtmlAttributeName("class")]
    public string? CustomClass { get; set; }

    [HtmlAttributeName("style")]
    public string? CustomStyle { get; set; }
    [HtmlAttributeName("form-radio")]
    public IEnumerable<SelectListItem>? RadioItems { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    private readonly IHtmlGenerator _generator;
    public AppInputTagHelper(IHtmlGenerator generator) => _generator = generator;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
      // <app-input> dış etiketini kaldır
      output.TagName = null;
      output.Attributes.Clear();

      var meta = For.Metadata;
      var name = For.Name;
      var id = TagBuilder.CreateSanitizedId(name, "_");

      /* -------- 1. LABEL -------- */
      var labelText = Label ?? meta.DisplayName ?? name;

      var labelTag = new TagBuilder("label");
      labelTag.Attributes["for"] = id;
      labelTag.AddCssClass("form-label");
      labelTag.InnerHtml.Append(labelText);

      /* -------- 2. ORTAK HTML ATTRIBUTES -------- */
      var htmlAttrs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
      {
        ["id"] = id,
        ["class"] = Items != null ? "form-select" : "form-control"
      };

      if (!string.IsNullOrWhiteSpace(CustomClass))
        htmlAttrs["class"] = $"{htmlAttrs["class"]} {CustomClass}";

      if (!string.IsNullOrWhiteSpace(CustomStyle))
        htmlAttrs["style"] = CustomStyle;

      if (Required is null or true)
        htmlAttrs["required"] = "required";

      // data-* / aria-* özniteliklerini aktar
      foreach (var attr in context.AllAttributes)
      {
        var key = attr.Name;
        if (key.StartsWith("data-", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("aria-", StringComparison.OrdinalIgnoreCase))
        {
          htmlAttrs[key] = attr.Value!;
        }
      }

      /* -------- 3. INPUT / SELECT / CHECKBOX -------- */
      IHtmlContent inputElement;

      // 3-a. <select>
      if (Items != null)
      {
        inputElement = _generator.GenerateSelect(
            viewContext: ViewContext,
            modelExplorer: For.ModelExplorer,
            expression: name,
            optionLabel: string.Empty,   // boş "—Seçiniz—" eklemeyeceğiz
            selectList: Items,
            allowMultiple: false,
            htmlAttributes: htmlAttrs);
      }
      // 3-b. Checkbox
      else if (meta.ModelType == typeof(bool) || meta.ModelType == typeof(bool?))
      {
        htmlAttrs["class"] = $"form-check-input{(string.IsNullOrWhiteSpace(CustomClass) ? "" : $" {CustomClass}")}";

        bool? isChecked = ValueOverride != null
            ? bool.TryParse(ValueOverride, out var tmp) ? tmp : (bool?)null
            : For.Model as bool?;

        var cb = _generator.GenerateCheckBox(
            viewContext: ViewContext,
            modelExplorer: For.ModelExplorer,
            expression: name,
            isChecked: isChecked,
            htmlAttributes: htmlAttrs);

        var cbLabel = new TagBuilder("label");
        cbLabel.AddCssClass("form-check-label");
        cbLabel.Attributes["for"] = id;
        cbLabel.InnerHtml.Append(labelText);

        var wrapper = new TagBuilder("div");
        wrapper.AddCssClass("form-check mb-3");
        wrapper.InnerHtml.AppendHtml(cb);
        wrapper.InnerHtml.AppendHtml(cbLabel);

        inputElement = wrapper;
      }
      else if(RadioItems!=null)
      {
        var radioWrapper = new TagBuilder("div");
        radioWrapper.AddCssClass("form-check-group");

        foreach (var item in RadioItems)
        {
          var radio = _generator.GenerateRadioButton(
              viewContext: ViewContext,
              modelExplorer: For.ModelExplorer,
              expression: name,
              value: item.Value,
              isChecked: item.Selected,
              htmlAttributes: new { @class = "form-check-input" });

          var radioLabel = new TagBuilder("label");
          radioLabel.AddCssClass("form-check-label");
          radioLabel.InnerHtml.Append(item.Text);

          var div = new TagBuilder("div");
          div.AddCssClass("form-check");
          div.InnerHtml.AppendHtml(radio);
          div.InnerHtml.AppendHtml(radioLabel);

          radioWrapper.InnerHtml.AppendHtml(div);
        }
        inputElement = radioWrapper;
      }
      // 3-c. Diğer tüm <input type="…">
      else
      {
        var inputType = meta.DataTypeName switch
        {
          "Password" => "password",
          "EmailAddress" => "email",
          "Url" => "url",
          "PhoneNumber" => "tel",
          "Date" => "date",
          "Time" => "time",
          _ when meta.ModelType == typeof(int)
                 || meta.ModelType == typeof(decimal)
                 || meta.ModelType == typeof(double)
              => "number",
          _ => "text"
        };

        htmlAttrs["type"] = inputType;

        if (!string.IsNullOrWhiteSpace(Placeholder))
          htmlAttrs["placeholder"] = Placeholder;

        var value = ValueOverride ?? For.Model;

        inputElement = _generator.GenerateTextBox(
            viewContext: ViewContext,
            modelExplorer: For.ModelExplorer,
            expression: name,
            value: value,
            format: null,
            htmlAttributes: htmlAttrs);
      }

      /* -------- 4. VALIDATION -------- */
      var validation = _generator.GenerateValidationMessage(
          viewContext: ViewContext,
          modelExplorer: For.ModelExplorer,
          expression: name,
          message: null,
          tag: null,
          htmlAttributes: new { @class = "invalid-feedback d-block" });

      /* -------- 5. ÇIKTIYI BİRLEŞTİR -------- */
      if (WrapLabel &&
          Items == null &&
          !(meta.ModelType == typeof(bool) || meta.ModelType == typeof(bool?)))
      {
        // Label input’u sarar
        var wrap = new TagBuilder("label");
        wrap.AddCssClass("form-label");
        wrap.InnerHtml.Append(labelText + " ");
        wrap.InnerHtml.AppendHtml(inputElement);
        output.Content.AppendHtml(wrap);
      }
      else
      {
        // Normal sıralama
        if (!(meta.ModelType == typeof(bool) || meta.ModelType == typeof(bool?)))
          output.Content.AppendHtml(labelTag);

        output.Content.AppendHtml(inputElement);
      }

      output.Content.AppendHtml(validation);
    }
  }
}
