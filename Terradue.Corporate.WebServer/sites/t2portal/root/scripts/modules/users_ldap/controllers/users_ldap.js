
define([
	'jquery',
	'can',
	'bootbox',
	'utils/crudBaseModule/crudBaseControl',
	'config',
	'utils/helpers',
	'modules/users_ldap/models/users_ldap',
	'messenger',
	'summernote',
	'datePicker',
	'dataTables'
], function($, can, bootbox, CrudBaseControl, Config, Helpers, UsersLdapModel){
	
	var UsersLdapControl = CrudBaseControl({}, {
		
		onIndex: function(element, options){
			var self = this;
		}
		
	});
		
	return new UsersLdapControl(Config.mainContainer, {
		Model: UsersLdapModel,
		entityName: 'users',
		view: '/scripts/modules/users_ldap/views/users_ldap.html',
		insertAtBeginning: true,
		loginDeferred: App.Login.isLoggedDeferred,
		adminAccess: true,
		dataTable: true
	});
	
});
