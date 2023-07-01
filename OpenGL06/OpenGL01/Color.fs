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

//�����
struct DirLight 
{
	vec3 direction;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

//���Դ
struct PointLight
{
	vec3 position;
	
	float constant;
	float linear;
	float quadratic;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

//�۹�
struct SpotLight
{
	vec3 direction;
	vec3 position;

	vec3 ambient;
	sampler2D diffuse;
	vec3 specular;

	float constant;
	float linear;
	float quadratic;

	float cutOff;
	float outerCutOff;
};

uniform Material material;
uniform DirLight dirLight;
uniform SpotLight spotLight;
#define NR_POINT_LIGHT 4
uniform PointLight pointLights[NR_POINT_LIGHT];

uniform vec3 viewPos;

//�������㺯��
vec3 CalcDirLight(DirLight light,vec3 normal,vec3 viewDir);

//���Դ���㺯��
vec3 CalcPointLight(PointLight light,vec3 normal,vec3 viewDir,vec3 FragPos);

//�۹��
vec3 CalcSpotLight(SpotLight light,vec3 normal,vec3 FragPos,vec3 viewDir);

void main()
{	
	vec3 normal = normalize(Normal);
	vec3 viewDir = normalize(viewPos - FragPos);

	//�����
	vec3 result = CalcDirLight(dirLight,normal,viewDir);

	//���Դ
	for(int i=0;i<NR_POINT_LIGHT;i++)
	{
		result += CalcPointLight(pointLights[i],normal,viewDir,FragPos);
	}

	//�ϲ�
	result += CalcSpotLight(spotLight, normal, FragPos, viewDir);    
    
    FragColor = vec4(result, 1.0);

}

//���㶨���
vec3 CalcDirLight(DirLight light,vec3 normal,vec3 viewDir)
{
	vec3 lightDir = normalize(-light.direction);

	//������
	float diff = max(dot(normal,lightDir),0.0);

	//�����
	vec3 reflectDir = reflect(-lightDir,normal);
	float spec=pow(max(dot(reflectDir,viewDir),0.0),material.shininess);

	//�ϲ�
	vec3 ambient = light.ambient * texture(material.diffuse,TexCoords).rgb;
	vec3 diffuse = light.diffuse * diff * texture(material.diffuse,TexCoords).rgb;
	vec3 specular = light.specular * spec * texture(material.specular,TexCoords).rgb;

	return ambient + diffuse + specular;
}

//������Դ
vec3 CalcPointLight(PointLight light,vec3 normal,vec3 viewDir,vec3 FragPos)
{
	vec3 lightDir = normalize(light.position - FragPos);

	//������
	float diff = max(dot(normal,lightDir),0.0);

	//�����
	vec3 reflectDir = reflect(-lightDir,normal);
	float spec = pow(max(dot(reflectDir,viewDir),0.0),material.shininess);

	//˥��
	float distance = length(light.position - FragPos);
	float attenuation = 1.0/(light.constant + light.linear * distance + light.quadratic * distance * distance);

	//�ϲ�
	vec3 ambient = light.ambient * texture(material.diffuse,TexCoords).rgb;
	vec3 diffuse = light.diffuse * diff * texture(material.diffuse,TexCoords).rgb;
	vec3 specular = light.specular *spec * texture(material.specular,TexCoords).rgb;
	ambient *= attenuation;
	diffuse *= attenuation;
	specular *= attenuation;

	return ambient + diffuse + specular;
}

vec3 CalcSpotLight(SpotLight light,vec3 normal,vec3 FragPos,vec3 viewDir)
{
	vec3 lightDir = normalize(light.position - FragPos);
	float theta = dot(normalize(-light.direction),lightDir);
	float epsilon = light.cutOff - light.outerCutOff;
	float intensity = clamp((theta - light.outerCutOff)/epsilon,0.0,1.0);

	//������
	float diff = max(dot(normal,lightDir),0.0);

	//�����
	vec3 reflectDir = reflect(-lightDir,normal);
	float spec = pow(max(dot(reflectDir,viewDir),0.0),material.shininess);

	//˥��
	float distance = length(light.position - FragPos);
	float attenuation = 1.0/(light.constant + light.linear * distance + light.quadratic * distance * distance);

	//�ϲ�
	vec3 ambient = light.ambient * texture(material.diffuse,TexCoords).rgb;
	vec3 diffuse = texture(spotLight.diffuse,-TexCoords).rgb * diff;
	vec3 specular = light.specular * spec * texture(material.specular,TexCoords).rgb;
	ambient *= attenuation * intensity;
	diffuse *= attenuation * intensity;
	specular *= attenuation * intensity;

	return ambient + diffuse + specular;
}