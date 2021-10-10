var umbraco = angular.module('umbraco');

umbraco.factory('fallbackTextService', ['$http', function ($http) {
    var baseUrl = '/Umbraco/FallbackText';

    function getTemplateData(nodeId, propertyAlias) {
        return $http.get(`${baseUrl}/TemplateData/Get?nodeId=${nodeId}&propertyAlias=${propertyAlias}`);
    };

    var service = {
        getTemplateData: getTemplateData
    };
    return service;
}]);