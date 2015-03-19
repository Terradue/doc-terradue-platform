
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
			
			errorCallback: null, // it's a function like function(['CREATE'|'READ'|'UPDATE'|'DELETE'], xhr)
			
			msgCreateSuccess: 'Item created.',
			msgCreateFail: 'Error during item creation.',
			
			msgReadSuccess: 'Items successfully loaded.',
			msgReadFail: 'Error during items loading.',
			
			msgUpdateSuccess: 'Item updated.',
			msgUpdateFail: 'Error during item update.',
			
			msgDeleteSuccess: 'Item removed.',
			msgDeleteFail: 'Error during item removing.',
		}
	},{
		
		init: function(element, options) {
			if (!options.Model){
				this.element.html('No model selected.');
				return;
			}
			
			this.log('init');			
			this.entities = new options.Model.List({});
			this.state = new can.Observe({
				entityName: options.entityName,
				entities: this.entities,
				showForm: false,
				selectedEntity: null,
			});
			
			if (this.init2)
				this.init2(element, options);
		},
		
		log: function(text){
			console.log('[CRUD] '+this.options.entityName+': '+text);
		},
		
		index: function(){
			if (!this.options.Model)
				return;

			this.log('index');

			this.element.html(can.view(this.options.view, this.state));
			
			if (this.index2)
				this.index2(element, options);
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
			//TODO: more than input elements
			$form.find('input').each(function(){
				if ($(this).attr('name'))
					data[$(this).attr('name')] = $(this).val();
			});
			$form.find('textarea').each(function(){
				if ($(this).attr('name'))
					data[$(this).attr('name')] = $(this).val();
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
			}
			
			if (this.entitySelected)
				this.entitySelected(entity);
			
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
					if (!id)
						self.entities.push(entity);
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

