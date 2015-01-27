<?xml version="1.0" encoding="UTF-8"?>
<!--
 This document is the property of Terradue and contains information directly 
 resulting from knowledge and experience of Terradue.
 Any changes to this code is forbidden without written consent from Terradue Srl
 
 Contact: info@terradue.com 
-->
<xsl:stylesheet version="2.0" 
	 xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >

<xsl:include href='site.xsl'/>

<xsl:template match="content" mode="head">
	<title><xsl:value-of select="$config/title" /> - People</title>
	<link rel="stylesheet" type="text/css" href="/style/css/people.css" />
	<script>
		$(document).ready(function() {				
			$("li#menu-people").addClass("active");
		});
	</script>
</xsl:template>


<xsl:template match="quote">
	<br/>		
	<div class="container">
		<div class="row-fluid">
				<blockquote class="span7">
		 			<p><i><xsl:value-of select="." /></i></p>
				</blockquote>
		</div>
	</div>
	
</xsl:template>	
	
<xsl:template match="itemList">
	
<div class="container">
	<div class="row-fluid">
		<div class="span12 area peopleArea">
			<p class="areaTitle">Meet the Team</p>
	
	<!-- iterate columns -->
	<xsl:for-each select="items/item[position() mod 3 = 1]">
		<!-- counter column (from 1 to 3) -->
		<xsl:variable name="vCurPos" select="(position()*3) - 2"/>
					
			<div class="row-fluid">
		
		<!-- iterate rows -->
		<xsl:for-each select="../item[position() >= $vCurPos and not(position() > $vCurPos + 2)]">

				<div class="span4 peopleAreaItem">
					<div class="peopleAreaItem">
						<img class="photo" src="{image}" />
						<div>
							<ul class="tags">
			<xsl:for-each select="tags/tag">
								<li><a href="{@url}" target="_blank">
									<xsl:value-of select="." />
								</a></li>
			</xsl:for-each>
							</ul>
							<div class="social">
				<xsl:if test="twitter != ''">
								<a href="{twitter}" target="_blank"><img src="/style/img/icons/twitter_small_icon.png" /></a>
				</xsl:if>
				<xsl:if test="linkedin != ''">
								<a href="{linkedin}" target="_blank"><img src="/style/img/icons/linkedin_small_icon.png" /></a>
				</xsl:if>
							</div>
						</div>
						<h4><xsl:value-of select="name" /></h4><br/>
						<!--<h5><xsl:value-of select="position" /></h5>-->
					</div>
				</div>
							
		</xsl:for-each>
			
			</div> <!-- row-fluid -->
					
	</xsl:for-each>

				
		</div>
	</div>
</div>
		
</xsl:template>

</xsl:stylesheet>