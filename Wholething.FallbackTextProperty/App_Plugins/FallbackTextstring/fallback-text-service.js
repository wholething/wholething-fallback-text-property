var umbraco = angular.module('umbraco');

umbraco.factory('fallbackTextService', ['$http', function ($http) {
    var baseUrl = '/Umbraco/FallbackText';

    function getTemplateData(nodeId, propertyAlias, culture) {
        return $http.get(`${baseUrl}/TemplateData/Get?nodeId=${nodeId}&propertyAlias=${propertyAlias}&culture=${culture}`).then(function (data) {
            return data.data;
        });
    };

    var service = {
        getTemplateData: getTemplateData
    };
    return service;
}]);