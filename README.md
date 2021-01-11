# Textstring with Fallback

This repository is a simple example of how to achieve a proper "fallback value" for an Umbraco field, including building that fallback from other node properties and properties of other specific nodes in the content tree.

The included example project uses the default Umbraco starter-kit and there are minimal changes.

## Configuration and editor experience

When you configure a "Textstring with fallback" property type you must configure a Mustache template to generate the fallback value.

![property-editor-config](https://user-images.githubusercontent.com/28703576/104169167-08869f80-53f7-11eb-9f98-de01fc23a72f.PNG)

In the example we use the following template: `{{1104:heroHeader}} - {{pageTitle}} - Bar`. In this case `pageTitle` refers to the nodes own property, `1104:heroHeader` refers to the site home node's property `heroHeader` and the rest is literal. The result can be seen below:

![editing-experience](https://user-images.githubusercontent.com/28703576/104169404-6ca96380-53f7-11eb-97ad-7c1ec1f3e543.PNG)

## Implementation

The implementation is fairly straight-forward and involves the following:
- The property editor builds a dictionary of node properties and their values and does the same for any other nodes mentioned in the template.
- The property editor renders the fallback template and stores that in the field value (the value stores is an object so that we can keep the fallback value distinct from an entered value)
- There is a value converter that tells Models Builder that the property type is a `FallbackValue`
- The `FallbackValue` object `Value` field returns the appropriate value (the entered value if present, otherwise the fallback)

## Limitations

There are a few notable limitations:
- The fallback template can only handle simple properties. For example referring to other fallback properties in a fallback template wouldn't work.
- The fallback template rendering does not currently use live values, just whatever is in the model when the editor is loaded.
- Referring to other nodes in the template by node ID is not ideal, it's a suitable proof of concept but I'd like to look at other ways.
