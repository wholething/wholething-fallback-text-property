# Wholething Fallback Text Property

This Umbraco package provides two custom text property editors, "Textstring with Fallback" and "Textarea with Fallback", that allow developers to enter "fallback value", rendered from a Mustache template.

The fallback can be built from other node properties and properties of other specific nodes in the content tree.

If you like this property editor you may also like [Wholething.FallbackImagePickerProperty](https://github.com/harry-gordon/wholething-fallback-image-picker-property).

## Use-case

The use-case for this package is simple: editors often want the option to override images/values but it is unclear what the default value is. We feel it's a significant improvement in experience for editors to be able to see the default value before deciding to override it.

## Team

This property editor is a collaboration between [Harry Gordon](https://www.linkedin.com/in/hejgordon/) and Wholething ([Dean Leigh](https://www.linkedin.com/in/deanleigh/?) and [Ault Nathanielsz](https://www.linkedin.com/in/ault-nathanielsz-01725b13/)).

## Installation

You can find the package on NuGet: https://www.nuget.org/packages/Wholething.FallbackTextProperty/

## Configuration and editor experience

When you configure a "Textstring with Fallback" or "Textarea with Fallback" property you must configure a Mustache template to generate the fallback value.

![fallback-text-1](https://user-images.githubusercontent.com/28703576/106004102-c625b980-60aa-11eb-8919-0fe27fe1f8bd.PNG)

In the example we use the following template: `{{1104:heroHeader}} - {{pageTitle}} - Bar`. In this case `pageTitle` refers to the nodes own property, `1104:heroHeader` refers to the site home node's property `heroHeader` and the rest is literal. The result can be seen below:

![fallback-text-2](https://user-images.githubusercontent.com/28703576/106004107-c6be5000-60aa-11eb-918f-8944f73fedf5.PNG)

### Referring to other nodes

The fallback template supports the following node references:
- Node ID: `{{1069:propertyAlias}}`
- Parent node: `{{parent:propertyAlias}}`
- Root node: `{{root:propertyAlias}}`
- Ancestor by content type alias: `{{ancestor(blogPost):propertyAlias}}`

## Implementation

The implementation is fairly straight-forward and involves the following:
- The property editor builds a dictionary of node properties and their values and does the same for any other nodes mentioned in the template.
- The property editor renders the fallback template but does not store that in the field value (to avoid "caching" depedent values).
- There is a value converter that returns either the entered value or renders the fallback value.

## Limitations

There are a few notable limitations:
- The fallback template can only handle simple properties. For example referring to other fallback properties in a fallback template wouldn't work.
- The fallback template rendering does not currently use live values, just whatever is in the model when the editor is loaded.
