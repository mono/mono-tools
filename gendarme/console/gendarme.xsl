<?xml version="1.0" encoding="iso-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="html" encoding="iso-8859-1" /> 
	<xsl:template name="print-defect-rules">
		<xsl:param name="name" />
		: <xsl:value-of select="count(//violation[@Name = $name])" /> defects
	</xsl:template>
	<xsl:template name="print-rules">
		<xsl:param name="type" />
			<p>
				<b><xsl:value-of select="$type" /></b>:
				<xsl:choose>
					<xsl:when test="count(rules/rule[@Type = $type]) = 0">
						<ul>
							<li>None</li>
						</ul>									
					</xsl:when>
					<xsl:otherwise>						
						<ul>
							<xsl:for-each select="rules/rule[@Type = $type]">
								<li>
									<a href="{@Uri}" target="{@Name}"><xsl:value-of select="text()" /></a>
									<xsl:call-template name="print-defect-rules">
										<xsl:with-param name="name">
											<xsl:value-of select="@Name" />
										</xsl:with-param>
									</xsl:call-template>
								</li>
							</xsl:for-each>
						</ul>						
					</xsl:otherwise>
				</xsl:choose>				
			</p>			
	</xsl:template>
	<xsl:template match="/">
		<xsl:for-each select="gendarme-output">
			<html>
				<head>
					<title>Gendarme Report</title>
				</head>
				<style type="text/css">
					h1, h2, h3 {
						font-family: Verdana;
						color: #68892F;
					}
					h2 {
						font-size: 14pt;
					}
					
					p, li, b {
						font-family: Verdana;
						font-size: 11pt;
					}			
					p.where, p.problem, p.found, p.solution {
						background-color: #F6F6F6;
						border: 1px solid #DDDDDD;
						padding: 10px;
					}
					span.found {
						padding: 10px;
					}
					div.toc {
						background-color: #F6F6F6;
						border: 1px solid #DDDDDD;
						padding: 10px;	
						float: right;				
						width: 300px;						
					}
					a:link, a:active, a:hover, a:visited {
						color: #9F75AD;
						font-weight: bold;
						text-decoration: none;
					}
				</style>
				<body>
					
					<h1>Gendarme Report</h1>
					<p>Produced on <xsl:value-of select="@date" /> UTC.</p>
					
					<div class="toc">
						<div align="center">
							<b style="font-size: 10pt;">Table of contents</b>
						</div>
						<p style="font-size: 10pt;">														
							<a href="#s1">1&#160;&#160;Summary</a><br />
							<a href="#s1_1">&#160;&#160;1.1&#160;&#160;List of assemblies searched</a><br />
							<a href="#s1_2">&#160;&#160;1.2&#160;&#160;List of rules used</a><br />
							<a href="#s2">2&#160;&#160;Reported defects</a><br />
						</p>						
					</div>
					<h1><a name="s1">Summary</a></h1>
					
					<p>
						<h2>List of assemblies analyzed</h2>
						<ul>
							<xsl:for-each select="input">
								<xsl:variable name="assembly"><xsl:value-of select="@Name" /></xsl:variable>
								
								<li><xsl:value-of select="text()" />: <xsl:value-of select="count(//violation[@Assembly = $assembly])" /> defects</li>
							</xsl:for-each>
						</ul>
					</p>
					
					<p>
						<h2>List of rules used</h2>
						
						<xsl:call-template name="print-rules">						
							<xsl:with-param name="type">Assembly</xsl:with-param>
						</xsl:call-template>
						
						<xsl:call-template name="print-rules">
							<xsl:with-param name="type">Module</xsl:with-param>
						</xsl:call-template>
						
						<xsl:call-template name="print-rules">
							<xsl:with-param name="type">Type</xsl:with-param>
						</xsl:call-template>
						
						<xsl:call-template name="print-rules">
							<xsl:with-param name="type">Method</xsl:with-param>
						</xsl:call-template>
					</p>
					
					<h1><a name="s2">Reported Defects</a></h1>
					
					<p>
						<xsl:for-each select="violation">
							<h3><xsl:value-of select="position()" />&#160;
								<a href="{@Uri}" target="{@Name}">
									<xsl:value-of select="@Name" />
								</a>
							</h3>

							<b>Problem:</b>
							<p class="problem">
								<xsl:value-of select="problem" />
							</p>

							<b>Found in:</b>
							<p class="found">
							Assembly Qualified Name: <i><xsl:value-of select="@Assembly" /></i><br/>
							<xsl:if test="count(messages/message) != 0">
								<xsl:for-each select="messages/message">
									<br/>
<!-- FIXME: use different color/style for warnings versus errors -->
									<b>Location:</b>&#160;<xsl:value-of select="@Location" />
									<br/>
									<span class="found">
										<xsl:value-of select="." />
									</span>
									<br/>
								</xsl:for-each>
							</xsl:if>
							</p>

							<b>Solution:</b>
							<p class="solution">
								<xsl:value-of select="solution" />
							</p>							
						</xsl:for-each>
					</p>
				</body>
			</html>
		</xsl:for-each>
	</xsl:template>
</xsl:stylesheet>
