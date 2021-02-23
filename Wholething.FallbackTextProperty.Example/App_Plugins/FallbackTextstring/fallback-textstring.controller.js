var umbraco = angular.module('umbraco');

umbraco.controller('FallbackTextstringController', ['$scope', 'assetsService', 'contentResource', 'editorState', function ($scope, assetsService, contentResource, editorState) {

    var templateDictionary = {};
    var template;

    var form = null;

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
                form.text.$setValidity("maxChars", true);
            } else {
                form.text.$setValidity("maxChars", false);
            }
        }
    };

    $scope.onUseValueChange = function () {
        // Annoyingly radio button value is a string
        $scope.useValue = $scope.useValueStr === 'true';

        $scope.none = $scope.useValueStr === '<none>';

        // If we are switching from custom to default let's "shelve" the custom value 
        // but bring it back if they go the other way
        if ($scope.none) {
            $scope.model.value = '<none>';
            $scope.useValue = null;
        } else if (!$scope.useValue) {
            $scope.model.value = null;
        } else {
            $scope.model.value = $scope.value;
        }
    };
    
    $scope.model.onValueChanged = $scope.change;

    function init() {
        $scope.allowNone = $scope.model.config.allowNone === '1';

        $scope.none = $scope.model.value === '<none>';

        // If the value is none but the field doesn't support it
        if (!$scope.allowNone && $scope.none) {
            $scope.none = false;
            $scope.model.value = null;
        }

        if ($scope.none) {
            $scope.useValue = null;
            $scope.useValueStr = '<none>';
        } else {
            $scope.useValue = $scope.model.value != null && $scope.model.value.length > 0;
            $scope.useValueStr = $scope.useValue.toString();
            $scope.value = $scope.model.value;
        }

        template = $scope.model.config.fallbackTemplate;

        initForm();

        // Add current node to the template dictionary
        addToDictionary(editorState.getCurrent());

        var otherNodeIds = getOtherNodeIds();

        var promises = otherNodeIds.map((nodeId) => {
            return new Promise((resolve) => {
                contentResource.getById(nodeId).then(function (node) {
                    addToDictionary(node, true);
                }).catch(function (err) {
                    console.log(`Couldn't find node mentioned in template (${nodeId})`);
                }).finally(function () {
                    resolve();
                });
            });
        });

        // Update fallback all the nodes have been loaded into the dictionary
        Promise.all(promises).then(() => {
            updateFallbackValue();
        });
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

    function updateFallbackValue() {
        $scope.fallback = Mustache.render(template, templateDictionary);
    }

    function addToDictionary(node, prefix) {
        var variant = node.variants[0];
        for (var tab of variant.tabs) {
            for (var property of tab.properties) {
                var key = prefix ? `${node.id}:${property.alias}` : property.alias;
                templateDictionary[key] = property.value;
            }
        }
    }

    function getOtherNodeIds() {
        var regex = new RegExp(/([0-9]+):/g);
        var matches = template.matchAll(regex);

        var nodeIds = [];
        for (var match of matches) {
            nodeIds.push(parseInt(match[1]));
        }

        return nodeIds;
    }

    function checkLengthValidity() {
        $scope.validLength = $scope.maxChars ? $scope.charsCount <= $scope.maxChars : true;
    }
}]);