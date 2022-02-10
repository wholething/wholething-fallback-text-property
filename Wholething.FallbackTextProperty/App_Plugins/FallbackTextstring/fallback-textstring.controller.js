var umbraco = angular.module('umbraco');

umbraco.controller('FallbackTextstringController', ['$scope', 'assetsService', 'contentResource', 'editorState', 'fallbackTextService', function ($scope, assetsService, contentResource, editorState, fallbackTextService) {

    var templateDictionary = null;
    var template;

    var form = null;

    var noneValue = '<none>';
    $scope.noneValue = noneValue;

    var templateDictionaryPromise = null;

    assetsService
        .load([
            '~/App_Plugins/FallbackTextstring/lib/mustache.min.js'
        ])
        .then(function () {
            init();
        });

    $scope.change = function () {
        $scope.model.value = $scope.value;

        if ($scope.model.value) {
            $scope.charsCount = $scope.model.value.length;
            checkLengthValidity();
            $scope.nearMaxLimit = $scope.validLength && $scope.charsCount > Math.max($scope.maxChars * .8, $scope.maxChars - 25);

            if ($scope.validLength === true) {
                form.text.$setValidity('maxChars', true);
            } else {
                form.text.$setValidity('maxChars', false);
            }
        }
    };

    $scope.onUseValueChange = function () {
        // Annoyingly radio button value is a string
        $scope.useValue = $scope.useValueStr === 'true';

        $scope.none = $scope.useValueStr === noneValue;

        // If we are switching from custom to default let's "shelve" the custom value 
        // but bring it back if they go the other way
        if ($scope.none) {
            $scope.model.value = noneValue;
            $scope.useValue = null;
        } else if (!$scope.useValue) {
            $scope.model.value = null;
        } else {
            $scope.model.value = $scope.value;
        }
    };

    $scope.model.onValueChanged = $scope.change;

    function init() {
        $('.umb-property-editor input').change(function () {
            updateFallbackDictionary();
        });

        $scope.allowNone = $scope.model.config.allowNone === '1';

        $scope.none = $scope.model.value === noneValue;

        // If the value is none but the field doesn't support it
        if (!$scope.allowNone && $scope.none) {
            $scope.none = false;
            $scope.model.value = null;
        }

        if ($scope.none) {
            $scope.useValue = null;
            $scope.useValueStr = noneValue;
        } else {
            $scope.useValue = $scope.model.value != null && $scope.model.value.length > 0;
            $scope.useValueStr = $scope.useValue.toString();
            $scope.value = $scope.model.value;
        }

        template = $scope.model.config.fallbackTemplate;

        initForm();

        templateDictionaryPromise = getFallbackDictionary();

        updateFallbackDictionary();
    }

    function initForm() {
        form = $scope.fallbackTextareaForm || $scope.fallbackTextstringForm;

        var isTextstring = !!$scope.fallbackTextstringForm;

        // macro parameter editor doesn't contains a config object,
        // so we create a new one to hold any properties
        if (!$scope.model.config) {
            $scope.model.config = {};
        }

        if (isTextstring) {
            // 512 is the maximum number that can be stored
            // in the database, so set it to the max, even
            // if no max is specified in the config
            $scope.maxChars = Math.min($scope.model.config.maxChars || 512, 512);
        } else {
            $scope.maxChars = $scope.model.config.maxChars;
        }

        $scope.charsCount = 0;
        $scope.nearMaxLimit = false;
        $scope.validLength = true;
    }

    function checkLengthValidity() {
        $scope.validLength = $scope.maxChars ? $scope.charsCount <= $scope.maxChars : true;
    }

    function getFallbackDictionary() {
        console.log(editorState.getCurrent(), $scope.model);
        return fallbackTextService.getTemplateData(editorState.getCurrent().id, $scope.model.alias, $scope.model.culture).then(function(data) {
            templateDictionary = data;
        }, function (error) {
            templateDictionary = {};
            console.log(error);
        });
    }

    function updateFallbackDictionary() {
        templateDictionaryPromise.then(function() {
            // Add current node to the template dictionary
            addToDictionary(editorState.getCurrent());

            updateFallbackValue();
        });
    }

    function updateFallbackValue() {
        // We need to avoid HTML encoding of things like ampersand
        var config = {
             escape: function (text) { return text; }
        }

        $scope.fallback = Mustache.render(template, templateDictionary, null, config);
    }

    function addToDictionary(node, addPrefix) {
        var prefix = addPrefix ? `${node.id}:` : '';
        var variant = getVariant(node);
        templateDictionary[buildKey('name', prefix)] = variant.name;
        for (var tab of variant.tabs) {
            for (var property of tab.properties) {
                templateDictionary[buildKey(property.alias, prefix)] = property.value;
            }
        }
    }

    function getVariant(node) {
        if (!$scope.model.culture) {
            return node.variants[0];
        }
        return node.variants.find(v => v.language.culture === $scope.model.culture);
    }

    function buildKey(alias, prefix) {
        return `${prefix}${alias}`;
    }
}]);