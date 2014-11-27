define([
	'jquery',
	'can',
	'geobrowser/config',
	'utils/helpers',
	'geobrowser/wpsList',
	'geobrowser/wpsJobsControl',
	'bootstrap',
	'multiFieldExtender',
	'wpsSlurper',
],function($, can, Config, Helpers, WpsListControl, WpsJobsControl){
	
	var WpsControl = can.Control({
		
		init: function(element, options){
			var self = this;
			
			element.html(can.view('/scripts/geobrowser/views/wps.html', {}));
			
			element.find('#wpsTab a').click(function(e){
				e.preventDefault();
				$(this).tab('show');
				
				self.triggerTabShow();
			});
			element.find('#wpsTab a[href="#servicesList"]').tab('show');
			
			// init wps services list
			this.wpsListControl = new WpsListControl(element.find('#servicesList'), {
				processSelected: function(wpsId, processId){
					self.processSelected(wpsId, processId);
					self.showAlgorithmForm();
				}
			});
			
			this.wpsJobsControl = new WpsJobsControl(element.find('#jobsList'), {
				jobSelected: function(job){
					self.processSelected(job.wpsIdentifier, job.processIdentifier);
					var $div = self.element.find('.form').hide();
					self.wpsJobsControl.showJob(job, $div);
					$div.fadeIn();
					self.wpsJobsControl.setPolling(true);
				}
			});
			window.wpsJobsControl = this.wpsJobsControl;
		},
		
		processSelected: function(wpsId, processId){
			var self = this,
				service = $.grep(self.wpsListControl.services, function(s){ return s.wpsIdentifier == wpsId; });
			
			if (service.length==0)
				return;
			
			var processDescription = service[0].wps.processes[processId];
			
			if (processDescription==null)
				return;
			
			this.element.find('#wpsTab, .tab-content').hide();
			this.element.find('#wpsProcessDetails').fadeIn()
				.html(can.view('/scripts/geobrowser/views/processDetails.html', processDescription));
			
			this.currentProcess = processDescription;
		},
		
		showAlgorithmForm: function(){
			var self = this,
				$div = this.element.find('#wpsProcessDetails .form'),
				$form = $("<form>").appendTo($div),
				processDescription = this.currentProcess;
			
			processDescription.parameters.each(function(){
				var p = this;
					$fieldContainer = $('<div class="fieldContainer"></div>').appendTo($form);
					
				$('<div class="dropMessage">'
						+'<i class="fa fa-plus-circle fa-2x"></i>'
						+' <span>Drop here to add data</span>'
						+'</div>').appendTo($fieldContainer);
				
				$fieldContainer.append("<label class='formLabel'>"+ p.title +"</label>");
				
				var $field = null;
				// check if we have a finite set of allowed values
				if (p.allowedValues){
					$field = $("<select class='wpsInput wpsInput-select'>");
					p.allowedValues.each(function(){
						$field.append("<option>"+this+"</option>");
					});
				} else if (p.isComplexData)
					$field = $('<input type="file" class="wpsInput wpsInput-file" />');
				else
					$field = $("<input type='text' class='wpsInput wpsInput-text' />");
				
				$field.attr({
					id: "field_" + p.identifier,
					name: p.identifier,
				});
				
				// adding popover
//				if (p.description && p.description!="" && p.description!=p.identifier)
//					$field.popover({
//						trigger: 'focus',
//						placement: 'top',
//						title: p.identifier,
//						content: p.description,
//					});

				if (p.maxOccurs==1) {
					// non-multiple field
					$fieldContainer.append($field);
				} else if (p.maxOccurs>1){
					// multiple field
					var $fieldset = $("<fieldset id='fieldset_" + p.identifier + "' name='" + p.identifier + "' class='wpsInput wpsInput-multiField'></fieldset>");
					$fieldset.append($field);
					$fieldContainer.append($fieldset);

					$fieldset.EnableMultiField({
						linkText: '',
						addMoreFieldsItem: ' <i class="fa fa-plus-circle addMoreFields"></i>',
						removeFieldsItem: ' <i class="fa fa-minus-circle removeFields"></i>',
						removeLinkText: '',
						confirmOnRemove: false,
						maxItemsAllowedToAdd: p.maxOccurs-1,
					});
				}
				
				// droppable
				$fieldContainer.droppable({
					accept: "#osResults tr, .featuresBasketTable",
					activeClass: "dropActive",
					hoverClass: "dropHover",
					drop: function(event, ui) {
						var features = (ui.draggable.hasClass('featuresBasketTable') ?
							self.options.getFeaturesFromBasket()
							: self.options.getFeaturesFromSelection()							
						)
						if (!features) return;
						
						if (p.maxOccurs==1)
							$field.val(features[0].id);
						else
							self.addFeaturesToFieldSet($fieldset, features);						
					}
				});

			});
			
			$form.append('<br/><button class="btn btn-info submit btn-small"><i class="fa fa-play-circle"></i> Run Job</button>');
		},
		
		addFeaturesToFieldSet: function($fieldset, features){
			var lastInput = $fieldset.parent().find('input').last();
			if (lastInput.val()==''){
				lastInput.val(this.getFeatureId(features[0].id));
				startIndex = 1;
			} else
				startIndex = 0;
			
			for (i=startIndex; i<features.length; i++){
				var $newField = $fieldset.addField();
				$newField.find('input').val(this.getFeatureId(features[i].id));
			}
		},
		
		addFeaturesToField: function($field, features){
			if (features.length==0)
				return;
			
			var lastInput = $fieldset.find('input').last();
			if (lastInput.val()==''){
				lastInput.val(this.getFeatureId(features[0].id));
				startIndex = 1;
			} else
				startIndex = 0;
			
			for (i=startIndex; i<features.length; i++){
				var $newField = $fieldset.addField();
				$newField.find('input').val(this.getFeatureId(features[i].id));
			}
		},
		
		getFeatureId: function(id){
			return id.replace('format=json', 'format=rdf');
		},
		
		getDataFromForm: function() {
			// get all form values as parameters
			var wpsParameters = "",
				currentProcess = this.currentProcess,
				ris = { parameters: [], isComplexData:false },
				$formArea = this.element.find(".form");
			
			$formArea.find('.fieldContainer>.wpsInput').each(function(){
				var $wpsInput = $(this);
				
				// multifield
				if ($wpsInput.hasClass('wpsInput-multiField')){
					var name = $wpsInput.attr("name"),
						value = $wpsInput.find("input, select").val();
				
				// text fields inputs and select (excluding multifield)
				} else if ($wpsInput.hasClass('wpsInput-text') || $wpsInput.hasClass('wpsInput-select')){
					var name = $wpsInput.attr("name"),
					value = $wpsInput.val();
					
				// radio group inputs
				} else if ($wpsInput.hasClass('wpsInput-radioGroup')){
					var $selectedRadio = $wpsInput.find("input[type='radio']:checked"),
					name = $selectedRadio.attr("name"),
					value = $selectedRadio.val();

				// checkboxes inputs
				} else if ($wpsInput.hasClass('wpsInput-multiField')){
					$formArea.find("form input.wpsInput-checkbox").each(function(){
						var $checkbox = $(this),
							name = $checkbox.attr("name"),
							checkedValue = ($checkbox.data('checked-value') ? $checkbox.data('checked-value') : true),
							uncheckedValue = ($checkbox.data('unchecked-value') ? $checkbox.data('unchecked-value') : false),
							value = $checkbox.prop('checked') ? checkedValue : uncheckedValue;
									
						wpsParameters += name + "=" + encodeURIComponent(value) + ";";
						ris.parameters.push( { name: name, value: value } );
					});
				}
				
				if (name!=null && value!=null && value!="") {
					wpsParameters += name + "=" + encodeURIComponent(value) + ";";
					ris.parameters.push( { Key: name, Value: value } );
				}
			});
			
			// set the url
			var executeBaseUrl = currentProcess.wps.operations.execute,
				interrogative = (executeBaseUrl[executeBaseUrl.length-1] == '?' ? '' : '?');
			
			ris.urlSync = executeBaseUrl + interrogative + "service=wps&version=" + currentProcess.wps.version +
				"&request=Execute&identifier="+ currentProcess.identifier +
				"&dataInputs=" + wpsParameters +
				"&ResponseDocument=" + currentProcess.responseDocument;
			
			ris.urlAsync = ris.urlSync + "&storeExecuteResponse=true&status=true";
			return ris;
		},
		
		triggerTabHide: function(){
			this.wpsJobsControl.setPolling(false); // disable wps polling
		},

		triggerTabShow: function(){
			// set polling if service list is enabled
			if ($('#jobsList').hasClass('active'))
				this.wpsJobsControl.setPolling(true); // enable wps polling
			else
				this.wpsJobsControl.setPolling(false); // disable wps polling
		},
		
		'#wpsProcessDetails .back click': function(){
			this.element.find('#wpsTab, .tab-content').fadeIn();
			this.element.find('#wpsProcessDetails').hide();
			this.triggerTabShow();
		},
		
		'button.submit click': function(){
			var self = this,
				element = this.element,
				currentProcess = this.currentProcess,
				formData = this.getDataFromForm();
			
			element.mask('Starting the job...');
			
			WpsSlurper.parseExecute(formData.urlAsync, function(ris){
				element.unmask();
				
				var job = self.wpsJobsControl.createJob(ris, currentProcess, formData);
				var $div = element.find('.form').hide();
				self.wpsJobsControl.showJob(job, $div);
				$div.fadeIn();
				
				self.wpsJobsControl.setPolling(true);
				
			}, function(jqXHR, textStatus, errorThrown){
				element.unmask();
				alert("FAIL!");
				console.log(jqXHR, textStatus, errorThrown);
			});
			
			return false;
		},
		
	});
	
	return WpsControl;
	
});
