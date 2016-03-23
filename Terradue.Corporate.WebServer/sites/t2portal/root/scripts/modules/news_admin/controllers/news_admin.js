
define([
	'jquery',
	'can',
	'bootbox',
	'utils/crudBaseModule/crudBaseControl',
	'config',
	'utils/helpers',
	'modules/news_admin/models/news_admin',
	'messenger',
	'summernote',
	'datePicker'
], function($, can, bootbox, CrudBaseControl, Config, Helpers, NewsModel){
	
	var NewsAdminControl = CrudBaseControl({}, {
		
		onIndex: function(element, options){
			var self = this;
		},
		
		onEntitySelected: function(news){
			Helpers.scrollTop();
			
			// destroy eventual previous summernote
			this.element.find('textarea[name="Content"]').summernote('destroy');
			
			// init summernote
			this.element.find('textarea[name="Content"]').summernote();
			
			this.element.find('input[name="Date"]').datepicker({
				format: "yyyy-mm-dd"
			});
		},
		
		onCreateClick: function(){
			Helpers.scrollTop();
			this.element.find('textarea[name="Content"]').summernote('reset');
		},
		
		'.setDateNow click': function(){
			this.element.find('input[name="Date"]').val((new Date()).toISOString());
		}
		
	});
	
	return new NewsAdminControl(Config.mainContainer, {
		Model: NewsModel,
		entityName: 'news',
		view: '/scripts/modules/news_admin/views/news_admin.html',
		insertAtBeginning: true,
		loginDeferred: App.Login.isLoggedDeferred,
		adminAccess: true
	});
	
});
