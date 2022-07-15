var umbraco = angular.module('umbraco');

umbraco.factory('fallbackTextService', ['$http', 'eventsService', 'notificationsService', function ($http, eventsService, notificationsService) {
    var baseUrl = '/Umbraco/Backoffice/FallbackText';

    var block = null;

    var editorOpenUnsubscribe = eventsService.on(
        'appState.editors.open',
        function (event, args) {
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

    function getTemplateData(nodeId, blockId, dataTypeKey, culture) {
        var url = `${baseUrl}/TemplateData/Get?nodeId=${nodeId}&dataTypeKey=${dataTypeKey}&culture=${culture}`;
        if (blockId) {
            url += `&blockId=${blockId}`;
        }
        return $http.get(url).then(function (data) {
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