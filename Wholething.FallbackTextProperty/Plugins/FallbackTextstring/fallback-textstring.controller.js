var umbraco = angular.module('umbraco');

umbraco.controller('FallbackTextstringController', ['$scope', 'assetsService', 'contentResource', 'editorState', function ($scope, assetsService, contentResource, editorState) {

    var templateDictionary = {};
    var template;

    assetsService
        .load([
            '~/App_Plugins/FallbackTextstring/lib/mustache.min.js'
        ])
        .then(function () {
            init();
        });

    $scope.onValueChange = function () {
        $scope.model.value = $scope.value;
    };

    $scope.onUseValueChange = function () {
        // Annoyingly radio button value is a string
        $scope.useValue = $scope.useValueStr === 'true';

        // If we are switching from custom to default let's "shelve" the custom value 
        // but bring it back if they go the other way
        if (!$scope.useValue) {
            $scope.model.value = null;
        } else {
            $scope.model.value = $scope.value;
        }
    };

    function init() {
        $scope.useValue = $scope.model.value != null && $scope.model.value.length > 0;
        $scope.useValueStr = $scope.useValue.toString();

        template = $scope.model.config.fallbackTemplate;

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
}]);