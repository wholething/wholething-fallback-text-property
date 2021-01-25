:: Copies the plugin contents from Example to the package. Assumed workflow is that all changes and testing happens in Example
:: and will need copied over to the package project.

Xcopy /E /I /Y .\Wholething.FallbackTextProperty.Example\App_Plugins\FallbackTextstring\ .\Wholething.FallbackTextProperty\Plugins\FallbackTextstring\
ECHO Copied all files from Example plugin folder to package plugin folder
PAUSE