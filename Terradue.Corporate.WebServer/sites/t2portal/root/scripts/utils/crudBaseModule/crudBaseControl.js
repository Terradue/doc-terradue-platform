
define([
	'jquery',
	'can',
	'app',
	'utils/helpers',
	'utils/baseControl',
	'config',
	'bootbox',
	'messenger',
], function($, can, App, Helpers, BaseControl, Config, Bootbox){
	
	return BaseControl({
		defaults: {
			canCreate: true,
			canView: true,
			canModify: true,
			canDelete: true,
			entityName: 'Entity',
			Model: null,
			view: '/scripts/utils/crudBaseModule/crudBaseView.html',
			idName: 'Id',
			
			msgCreateSuccess: 'Item created.',
			msgCreateFail: 'Error during item creation.',
			
			msgReadSuccess: 'Items successfully loaded.',
			msgReadFail: 'Error during items loading.',
			
			msgUpdateSuccess: 'Item updated.',
			msgUpdateFail: 'Error during item update.',
			
			msgDeleteSuccess: 'Item removed.',
			msgDeleteFail: 'Error during item removing.',
			
			permissionCheck: null, // deferred
		}
	},{
		
		init: function(element, options) {
			if (!options.Model){
				this.element.html('No model selected.');
				return;
			}
			
			this.log('init');
			
			if (this.onInit)
				this.onInit(element, options);
		},
		
		log: function(text){
			console.log('[CRUD] '+this.options.entityName+': '+text);
		},
		
		index: function(){
			var self = this;
			
			if (!this.options.Model)
				return;

			this.log('index');
			
			var callback = function(){
				self.entities = new self.options.Model.List({});
				self.state = new can.Observe({
					entityName: self.options.entityName,
					entities: self.entities,
					showForm: false,
					selectedEntity: null,
				});
				
				self.element.html(can.view(self.options.view, self.state));

				if (self.onIndex)
					self.onIndex(self.element, self.options);
			};
			
			if (this.options.loginDeferred)
				this.options.loginDeferred
					.then(function(usr){
						if (self.options.adminAccess && !App.Login.isAdmin())
							self.accessDenied();
						else
							callback();
					})
					.fail(function(){
						// no logged
						self.accessDenied();
					});
			else
				// free access
				callback();
		},
				
		getEntityById: function(id){
			if (id==null)
				return null;
			
			var self = this,
				idName = this.options.idName,
				search = this.entities.filter(function(entity){return (entity[idName] == id)});
			
			if (search.length)
				return search[0];
			else
				return null;
		},
		
		getDataFromForm: function($form){
			var data = {};
			$form.find('input, textarea, select').each(function(){
				if ($(this).attr('name'))
					// consider the generic case (get the val) and the checkbox case (get the checked status)
					data[$(this).attr('name')] = ($(this).attr('type')=='checkbox') ? $(this).prop('checked').toString() : $(this).val();
			});
			return data;
		},
		
		successMessage: function(operation){
			if (this.options.successCallback)
				this.options.successCallback(operation);
			else {
				var msg = null;
				if (operation=='CREATE')
					msg = this.options.msgCreateSuccess;
				else if (operation=='READ')
					msg = this.options.msgReadSuccess;
				else if (operation=='UPDATE')
					msg = this.options.msgUpdateSuccess;
				else if (operation=='DELETE')
					msg = this.options.msgDeleteSuccess;
				
				if (msg)
					Messenger().post({
						message: msg, 
						type: 'success',
						showCloseButton: true,
						hideAfter: 4,
					});
			}
		},
		
		failMessage: function(operation, xhr){
			if (this.options.failCallback)
				this.options.failCallback(operation);
			else {
				var msg = null;
				if (xhr.responseJSON && xhr.responseJSON.ResponseStatus && xhr.responseJSON.ResponseStatus.Message)
					msg = xhr.responseJSON.ResponseStatus.Message;
				else if (xhr.responseJSON && response.responseJSON.Message)
					msg = xhr.responseJSON.Message;
				else if (xhr.responseText)
					msg = xhr.responseText;
				else{
					if (operation=='CREATE')
						msg = this.options.msgCreateFail;
					else if (operation=='READ')
						msg = this.options.msgReadFail;
					else if (operation=='UPDATE')
						msg = this.options.msgUpdateFail;
					else if (operation=='DELETE')
						msg = this.options.msgDeleteFail;
				}
				
				if (msg)
					Messenger().post({
						message: msg, 
						type: 'error',
						showCloseButton: true,
						hideAfter: 4,
					});
			}
		},
		
		'.entity .update click': function($el){
			var self = this,
				id = $el.closest('.entity').data('id'),
				entity = this.getEntityById(id);
			this.log('update '+id);			
			
			if (entity){
				console.log(entity);
				this.state.attr({
					showForm: true,
					selectedEntity: entity,
				});
				if (this.onEntitySelected)
					this.onEntitySelected(entity);
			}
			return false;
		},
		
		'.entity .delete click': function($el){
			var self = this,
				options = this.options,
				id = $el.closest('.entity').data('id'),
				entity = this.getEntityById(id);
			
			this.log('delete '+id+'!');
			
			if (entity){
				console.log(entity);
				Bootbox.confirm('Are you sure you want to remove the item?', function(confirm){
					if (confirm){
						entity.destroy().then(function(res){
							self.successMessage('DELETE');
						}).fail(function(xhr){
							self.failMessage('DELETE', xhr);
						});						
					}						
				});
			}
			return false;
		},
		
		'.openCreate click': function($el){
			this.state.attr({
				showForm: true,
				selectedEntity: null,
			});
			if (this.onCreateClick)
				this.onCreateClick();

			return false;
		},
		
		'.form .save click': function($el){
			var self = this,
				options = this.options,
				$form = this.element.find('.form'),
				id = $form.data('id'),
				formData = this.getDataFromForm($form),
				Model = this.options.Model;
				
			if (id){
				// update
					entity = this.getEntityById(id);
				if (entity)
					entity.attr(formData);
			} else
				// create
				var entity = new Model(formData);
			
			if (entity)
				entity.save().then(function(){
					self.log('saved!');
					self.state.attr('showForm', false);
					if (!id){
						if (self.options.insertAtBeginning)
							self.entities.unshift(entity);
						else
							self.entities.push(entity);
					}
						
					self.successMessage(id ? 'UPDATE' : 'CREATE');
				}).fail(function(xhr){
					self.failMessage(id ? 'UPDATE' : 'CREATE', xhr);
				});				
			
			return false;
		},
		
		'.form .cancel click': function($el){
			this.state.attr('showForm', false);
			return false;
		},
		
	});
});

