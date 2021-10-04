:: Copies the plugin contents from Example to the package. Assumed workflow is that all changes and testing happens in Example
:: and will need copied over to the package project.

Xcopy /E /I /Y .\Wholething.FallbackTextProperty.Examples.Umbraco9\App_Plugins\FallbackTextstring\ .\Wholething.FallbackTextProperty\App_Plugins\FallbackTextstring\
Xcopy /E /I /Y .\Wholething.FallbackTextProperty.Examples.Umbraco9\App_Plugins\FallbackTextstring\ .\Wholething.FallbackTextProperty.Examples.Umbraco9\App_Plugins\FallbackTextstring\
ECHO Copied all files from Umbraco 9 example plugin folder to package plugin folder
PAUSE