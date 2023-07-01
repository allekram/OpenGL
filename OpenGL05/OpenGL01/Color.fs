#version 330 core

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

out vec4 FragColor;

struct Material
{
	sampler2D diffuse;
	sampler2D specular;
	float shininess;
};

struct Light
{
	vec3 position;
	vec3 direction;
	float cutOff;
	float outerCutOff;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float constant;
	float linear;
	float quadratic;
};

uniform Material material;
uniform Light light;

uniform vec3 viewPos;

void main()
{	
	//ambient
	vec3 ambient = light.ambient * vec3(texture(material.diffuse,TexCoords));

	//diffuse
	vec3 normal = normalize(Normal);
	vec3 lightDir = normalize(light.position - FragPos);
	float diff = max(dot(normal,lightDir),0.0);
	vec3 diffuse = light.diffuse * diff * texture(material.diffuse,TexCoords).rgb;

	//specular
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflecDir = reflect(-lightDir,normal);
	float spec=pow(max(dot(viewDir,reflecDir),0.0),material.shininess);
	vec3 specular = light.specular * spec * vec3(texture(material.specular,TexCoords));

	//À•ºıº∆À„
	float distance = length(light.position - FragPos);
	float attenuation = 1.0/(light.constant + light.linear * distance + light.quadratic * distance * distance);
	diffuse *= attenuation;
	ambient  *= attenuation; 
	specular *= attenuation;

	float theta = dot(lightDir , normalize(-light.direction));
	float epsilon = (light.cutOff - light.outerCutOff);
	float intensity = clamp((theta - light.outerCutOff) / epsilon,0.0,1.0);
	diffuse  *= intensity;
    specular *= intensity;

	
	vec3 result = ambient + diffuse + specular;
	FragColor = vec4(result,1.0);

}