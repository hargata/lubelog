using CarCareTracker.Helper;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CarCareTracker.TagHelper;

[HtmlTargetElement("application-logo")]
public class ApplicationLogoTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
{
    [HtmlAttributeName("variant")]
    public string? Variant { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var path = Variant is "small"
            ? StaticHelper.DefaultSmallLogoPath
            : StaticHelper.DefaultLogoPath;
        var viewBox = Variant is "small"
            ? "0 0 16 16"
            : "0 0 111 16";

        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.Add("xmlns", "http://www.w3.org/2000/svg");
        output.Attributes.Add("fill", "currentColor");
        output.Attributes.Add("viewBox", viewBox);
        output.Content.AppendHtml(await File.ReadAllTextAsync($@"wwwroot{path}.txt"));
    }
}

// xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 111 16"
// xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 16 16"
