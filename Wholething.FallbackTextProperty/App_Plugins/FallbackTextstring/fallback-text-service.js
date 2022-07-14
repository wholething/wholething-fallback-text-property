var umbraco = angular.module('umbraco');

umbraco.factory('fallbackTextService', ['$http', 'eventsService', 'notificationsService', function ($http, eventsService, notificationsService) {
    var baseUrl = '/Umbraco/Backoffice/FallbackText';

    var block = null;

    var editorOpenUnsubscribe = eventsService.on(
        'appState.editors.open',
        function (event, args) {
            console.log(args);
            if (args.editor.view.includes('blockeditor')) {
                if (block) {
                    notificationsService.error(
                        'Fallback editor',
                        'Block editor opened from inside a block, fallback editor will not function correctly'
                    );
                }
                block = args.editor.content;
            }
        }
    );

    var editorCloseUnsubscribe = eventsService.on(
        'appState.editors.close',
        function (event, args) {
            if (args.editor.view.includes('blockeditor')) {
                block = null;
            }
        }
    );

    function getTemplateData(nodeId, propertyAlias, culture) {
        console.log('getTemplateData', nodeId, propertyAlias);
        return $http.get(`${baseUrl}/TemplateData/Get?nodeId=${nodeId}&propertyAlias=${propertyAlias}&culture=${culture}`).then(function (data) {
            return data.data;
        });
    };

    function getBlock() {
        return block;
    }

    var service = {
        getTemplateData: getTemplateData,
        getBlock: getBlock
    };
    return service;
}]);