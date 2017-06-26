
define([
	'jquery',
	'can',
	'bootbox',
	'utils/crudBaseModule/crudBaseControl',
	'config',
	'utils/helpers',
	'modules/users_ldap/models/users_ldap',
	'modules/users_admin/models/users_admin',
	'messenger',
	'summernote',
	'datePicker',
	'dataTables'
], function($, can, bootbox, CrudBaseControl, Config, Helpers, UsersLdapModel, UserModel){
	
	var UsersLdapControl = CrudBaseControl({}, {
		
		onIndex: function(element, options){
			var self = this;
		},

		'.createAccount click': function(el){
			var ldapUsername = el.data('username');
			UserModel.create({Username:ldapUsername})
				.then(function(user){
					//TODO: UPDATE the user row
				}).fail(function(){
					self.errorView({}, 'Unable to Create user ' + ldapUsername, '', true);
				});
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
