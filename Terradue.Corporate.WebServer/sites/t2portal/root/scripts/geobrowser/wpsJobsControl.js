define([
	'jquery',
	'can',
	'geobrowser/config',
	'geobrowser/models/wpsJob',
	'utils/helpers',
	'wpsSlurper',
	'prettify',
	'vkbeautify',
],function($, can, Config, WpsJobModel, Helpers){
	
	var WpsJobsControl = can.Control({
		
		init: function(element, options){
			var self = this;
			
			console.log("wpsJobs.init");
			
			this.jobs = new can.Observe.List([]);
			this.jobNames = [];
			this.pollingEnabled = false;
			this.pagination = new can.Observe({offset:0, limit:Config.wpsJobsPerPage});
			
			WpsJobModel.findAll({}, function(jobsList){
				
				var jobsList = jobsList.attr().sort(function(j1,j2){
					if (j1.CreatedTime>=j2.CreatedTime)
						return -1;
					else
						return 1;
				});
				
				$.each(jobsList, function(){
					var j = this;
					self.jobs.push({
						jobId: j.Identifier,
						jobName: j.Name,
						status: 'TO_CHECK',
						isSync: false,
						time: j.CreatedTime,
						parameters: j.parameters,
						statusLocation: j.StatusLocation,
						uuid: Helpers.randomId(20),
						wpsIdentifier: j.WpsId,
						processIdentifier: j.ProcessId,
						parameters: j.Parameters,
					});
				});
				
				self.updateLastJobsFromPageView();
				self.pagination.bind('change', function(ev, attr, how, newVal, oldVal){
					self.updateLastJobsFromPageView();
				});

			});
			
			element.html(can.view('/scripts/geobrowser/views/jobs.html', { 
					jobs: self.jobs,
					pagination: self.pagination
				}, {
					ifInOffset: function(index, offset, limit, options){
						var index = (index==null || typeof(index)!='function' ? index : index()),
							offset = (offset==null || typeof(offset)!='function' ? offset : offset()),
							limit = (limit==null || typeof(limit)!='function' ? limit : limit());
						return ((index>=offset && index<offset+limit) ? options.fn() : null);
					},
					showPagination: function(offset, limit, n){
						var offset = (offset==null || typeof(offset)!='function' ? offset : offset()),
							limit = (limit==null || typeof(limit)!='function' ? limit : limit()),
							n = (n==null || typeof(n)!='function' ? n : n()),
							nPages = Math.ceil(n/limit),
							ris = '';
						
						if (nPages>1)
							for (var i=0; i<n; i+=limit)
								if (i==offset)
									ris += '<i class="icon-circle"></i>';
								else
									ris += '<a class="changePage" href="javascript://" data-offset="' + i + '"><i class="icon-circle-blank"></i></a>'
						
						return new can.mustache.safeString(ris);
						
					},
				}
			));
			
			this.checkStatusPolling();
			
		},
		
		checkStatusPolling: function() {
			var self = this;
			setTimeout(function(){
				if (self.pollingEnabled){
					self.jobs.each(function(job){
						
						// check the status only if:
						// 1) the job is async;
						// 2) the statusLocation is set;
						// 3) the status is RUNNING;
						
						if (job.statusLocation && job.status=='RUNNING')
							self.updateJobStatus(job);						
					});
				}
				self.checkStatusPolling();
			}, Config.wpsPollingTime);
		},
		
		setPolling: function(flag){
			this.pollingEnabled = flag;
		},
		
		updateJobStatus: function(job){
			if (!job.statusLocation){
				job.attr({ status: 'ERROR', isTerminated: true });
				return;
			}
			
			WpsSlurper.parseStatus(job.statusLocation, function(statusResult){
				
				if (statusResult.isSucceeded || statusResult.isFailed){
					var xmlString='';
					
					if (window.ActiveXObject) 
						xmlString = $xml.xml; 
					else {
						var oSerializer = new XMLSerializer(); 
						xmlString = oSerializer.serializeToString(statusResult.$statusResult[0]);
					}
					xmlString = vkbeautify.xml(xmlString, 3);
				}
				
				if (statusResult.isSucceeded)
					job.attr({
						status:"SUCCESS",
						isTerminated:true,
						xmlString: xmlString,
						$xml:statusResult.$statusResult
					});
				
				else if (statusResult.isFailed)
					job.attr({
						status:"ERROR", 
						isTerminated:true,
						xmlString: xmlString,
						$xml: statusResult.$statusResult,
						exceptionText: statusResult.exceptionText
					});
				
				else if (statusResult.isRunning)
					job.attr({
						status: 'RUNNING',
						percent: statusResult.percent
					});

			}, function(){
				console.log("fail get wps status");
				job.attr({ status: 'ERROR', isTerminated: true });
				return;
			});
		},
		
		updateLastJobsFromPageView: function(){
			var offset = this.pagination.offset,
				limit = this.pagination.limit,
				self = this;
			
			this.jobs.each(function(job, i){
				if (i<offset || i>offset+limit-1 || job.status!='TO_CHECK')
					return true;
				else
					self.updateJobStatus(job);				
			});
		},
		
		showJob: function(job, $div){
			$div.html(can.view('/scripts/geobrowser/views/jobDetails.html', job));
			
//			var $xmlResult = $div.find('.xmlResult'),
//				$showXmlResultBtn = $div.find('.showXmlResultBtn')
//					.click(function(){
//						$(this).text(($xmlResult.css("display")=="none" ? "Hide": "Show") +  " the XML result.");
//						$xmlResult.toggle();
//						
//						return false;
//					});
			prettyPrint();
			job.bind('status',function(event, newVal, oldVal){
				if (oldVal=='RUNNING' && (newVal=='SUCCESS' || newVal=='ERROR'))
					prettyPrint();
			});
		},
		
		createJob: function(executeResult, process, formData){
			// create an unique jobName
			var jobName = process.title;
			if (this.jobNames[jobName]==null)
				this.jobNames[jobName]=1;
			else
				jobName += " ("+(this.jobNames[jobName]++)+")";
			
			var job = new can.Observe({
				jobId: executeResult.jobId,
				jobName: jobName,
				title: process.title,
				status: 'PENDING',
				percent: 0,
				isTerminated: false,
				time: new Date(),
				active: false,
				parameters: formData.parameters,
				url: formData.url,
				$executeResult: executeResult.$executeResult,
				statusLocation: executeResult.statusLocation,
				exceptionText: executeResult.exceptionText,
				uuid: Helpers.randomId(20),
				wpsIdentifier: process.wps.service.wpsIdentifier,
				processIdentifier: process.identifier,
				parameters: formData.parameters,
			});
			this.jobs.push(job);
			
			if (executeResult.statusLocation)
				job.attr({ status: 'RUNNING' });
			else {
				var oSerializer = new XMLSerializer(), 
					xmlString = oSerializer.serializeToString(executeResult.$executeResult[0]);
				job.attr({ status: 'ERROR', isTerminated: true, xmlString: xmlString });
			}
			
			// adding the job to the service
			if (executeResult.statusLocation)
				new WpsJobModel({
					WpsId: job.wpsIdentifier,
					ProcessId: job.processIdentifier,
					StatusLocation: job.statusLocation,
					CreatedTime: job.time,//"\/Date(-62135596800000-0000)\/",
					Identifier: (job.jobId ? job.jobId : job.uuid),
					Name: jobName,
					Parameters: job.parameters,
				}).save();
			
			return job;

		},
		
		'.selectJob click': function(el){
			if (this.options.jobSelected){
				var uuid = el.data('uuid'),
					self = this,
					job = $.grep(self.jobs, function(j){ return j.uuid == uuid; });
				
				if (job.length>0)
					this.options.jobSelected(job[0]);//el.data('wps-id'), el.data('process-id'), el.data('job-id')
			}
		},
		
		'a.changePage click': function(el){
			var offset = el.data('offset');
			this.pagination.attr('offset', offset);
		}
		
	});
	
	return WpsJobsControl;
	
});
