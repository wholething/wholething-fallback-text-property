var umbraco = angular.module('umbraco');

umbraco.controller('FallbackTextstringController', ['$scope', 'assetsService', 'contentResource', 'editorState', function ($scope, assetsService, contentResource, editorState) {

    var view = {};

    assetsService
        .load([
            '~/App_Plugins/FallbackTextstring/lib/mustache.min.js'
        ])
        .then(function () {
            init();
        });

    function init() {
        if (!$scope.model.value.value) {
            $scope.model.value = {
                value: '',
                fallback: null
            }
        }

        var node = editorState.getCurrent();
        var variant = node.variants[0];
        for (var tab of variant.tabs) {
            for (var property of tab.properties) {
                view[property.alias] = property.value;
            }
        }

        update();
    }

    function update() {
        $scope.model.value.fallback = Mustache.render($scope.model.config.fallbackTemplate, view);
    }
}]);