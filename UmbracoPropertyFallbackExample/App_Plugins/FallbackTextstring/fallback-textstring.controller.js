var umbraco = angular.module('umbraco');

umbraco.controller('FallbackTextstringController', ['$scope', function($scope) {

    init();

    $scope.placeholder = 'Placeholder text!';

    function init() {
    }
}]);